using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebApplication8.Models;

namespace WebApplication8.Areas.doctor.Controllers
{
    public class drugController : Controller
    {
        private DataContext db = new DataContext();

        public ActionResult Index()
        {
            
            if (Session["doc_id"] == null)
            {
                return RedirectToAction("Index", "user", new { area = "" });
            }
            return View(db.drugs.Where(x => x.org != x.phar &&
            x.drug_effective.Count != 0 && x.risk_conflict.Count != 0).OrderByDescending(x=>x.ScientificName));
        }
        [HttpPost]
        public ActionResult Index(string option, string search)
        {
            if (option == "TradeName")
            {
                return View(db.drugs.Where(x => x.TradeName.Contains(search) &&
                x.org != x.phar && x.drug_effective.Count != 0 && x.risk_conflict.Count != 0));
            }
            else if (option == "ScientificName")
            {
                return View(db.drugs.Where(x => x.ScientificName.Contains(search) &&
                x.org != x.phar && x.drug_effective.Count != 0 && x.risk_conflict.Count != 0));
            }
            else if (option == "effective")
            {
                List<effective> effectives = db.effectives.Where(x => x.name.Contains(search)).ToList();
                Session["a"] = effectives;
                return RedirectToAction("searchByEffective");
            }
            return View();
        }
        public ActionResult Details(int id)
        {
            drug drug = db.drugs.Find(id);
            if (drug == null)
            {
                return HttpNotFound();
            }
            return View(drug);
        }

        public ActionResult UpdateInfo()
        {
            int id = (int)Session["doc_id"];
            Session["name"] = db.users.Find(id).name;
            UpdateUser updateUser = new UpdateUser
            {
                Name = Session["name"] as string
            };
            return View(updateUser);
        }
        [HttpPost]
        public ActionResult UpdateInfo(UpdateUser updateUser)
        {
            updateUser.Name = Session["name"] as string;
            if (ModelState.IsValid)
            {
                int id = (int)Session["doc_id"];
                user user = db.users.Find(id);
                if (user.password == updateUser.OldPassword)
                {
                    user.password = updateUser.NewPassword;
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    ViewBag.error = "old password is wrong";
                    updateUser.OldPassword = null;
                    return View(updateUser);
                }
                return RedirectToAction("Index");
            }
            return View(updateUser);
        }


        public ActionResult searchByEffective()
        {
            List<effective> effectives = Session["a"] as List<effective>;
            return View(effectives);
        }
        public ActionResult showDrugs(int id)
        {
            List<drug_effective> drug_Effectives = db.drug_effective.Where(x => x.effective_id == id).ToList();
            List<drug> drugs = new List<drug>();
            foreach (drug_effective drug_Effective in drug_Effectives)
            {
                if (drug_Effective.drug.org != drug_Effective.drug.phar && drug_Effective.drug.drug_effective.Count != 0 && drug_Effective.drug.risk_conflict.Count != 0)
                {
                    drugs.Add(drug_Effective.drug);
                }
            }
            return View(drugs);
        }
        public ActionResult Create()
        {
            ViewBag.drugsList = new SelectList(db.drugs.Where(x => x.phar != x.org).OrderBy(x => x.ScientificName), "id", "ScientificName");
            return View();
        }
        [HttpPost]
        public ActionResult Create(string first, string second)
        {

            ViewBag.drugsList = new SelectList(db.drugs.Where(x => x.phar != x.org).OrderBy(x => x.ScientificName), "id", "ScientificName");
            if (string.IsNullOrEmpty(first) && string.IsNullOrEmpty(second))
            {

                ViewBag.error = "enter the first drug";
                ViewBag.errorr = "enter the second drug";
                return View();
            }
            if (string.IsNullOrEmpty(first))
            {
                ViewBag.error = "enter the first drug";
                return View();
            }

            drug old_drug = db.drugs.Find(Convert.ToInt32(first));

            if (string.IsNullOrEmpty(second))
            {
                ViewBag.errorr = "enter the second drug";
                return View();
            }

            drug new_drug = db.drugs.Find(Convert.ToInt32(second));

            if (old_drug == null || old_drug.phar == old_drug.org)
            {
                ViewBag.error = "first drug not found";
                return View();
            }
            if (new_drug == null || new_drug.phar == new_drug.org)
            {
                ViewBag.errorr = "second drug not found";
                return View();
            }
            if (old_drug == new_drug)
            {
                ViewBag.equal = "two drugs are same";
                return View();
            }
            int s = old_drug.id;
            int g = new_drug.id;
            List<drug_effective> old_drug_Effectives = db.drug_effective.Where(x => x.drug_id == s).ToList();
            List<drug_effective> new_drug_Effectives = db.drug_effective.Where(x => x.drug_id == g).ToList();
            List<int> old_effctive_ids = new List<int>();
            List<int> new_effctive_ids = new List<int>();
            List<effective> old_effective = new List<effective>();
            List<effective> new_effective = new List<effective>();
            foreach (drug_effective i in old_drug_Effectives)
            {
                old_effective.Add(i.effective);
                old_effctive_ids.Add(i.effective_id);
            }
            foreach (drug_effective i in new_drug_Effectives)
            {
                new_effective.Add(i.effective);
                new_effctive_ids.Add(i.effective_id);
            }
            Drug_eff first_drug = new Drug_eff
            {
                drug = old_drug,
                Effectives = old_effective
            };
            Session["first_drug"] = first_drug;
            Drug_eff second_drug = new Drug_eff
            {
                drug = new_drug,
                Effectives = new_effective
            };
            Session["second_drug"] = second_drug;
            foreach (int i in old_effctive_ids)
            {
                List<conflict> conflicts = db.conflicts.Where(x => x.effective_id == i || x.effective_id_con == i).ToList();
                foreach (int x in new_effctive_ids)
                {
                    foreach (conflict con in conflicts)
                    {
                        if (x == con.effective_id_con || x == con.effective_id)
                        {
                            return RedirectToAction("conflict");
                        }
                    }
                }
            }
            return RedirectToAction("NoConflict");
        }

        [HttpPost]
        public ActionResult AddDrug(waiting waiting)
        {
            if (string.IsNullOrEmpty(waiting.name) == false)
            {
                if (db.drugs.FirstOrDefault(x => x.TradeName == waiting.name.ToLower()) != null)
                {
                    TempData["x"] = "this drug is exist";
                    return RedirectToAction("Create");
                }
                if (db.waitings.FirstOrDefault(x => x.name == waiting.name.ToLower()) != null)
                {
                    TempData["x"] = "this drug is added to waiting list";
                    return RedirectToAction("Create");
                }
                waiting.name = waiting.name.ToLower();
                db.waitings.Add(waiting);
                db.SaveChanges();
                TempData["x"] = "added successfully";
                return RedirectToAction("Create");
            }
            TempData["x"] = "Name Required";
            return RedirectToAction("Create");
        }

        public ActionResult conflict()
        {
            var first_drug = (Drug_eff)Session["first_drug"];
            var second_drug = (Drug_eff)Session["second_drug"];
            List<risk_conflict> first_conflicts = first_drug.drug.risk_conflict.ToList();
            List<risk_conflict> second_conflicts = second_drug.drug.risk_conflict.ToList();
            conf_drug conf_Drug = new conf_drug
            {
                first_drug = first_drug.drug,
                first_effectives = first_drug.Effectives,
                risk_Conflicts = first_conflicts,
                second_drug=second_drug.drug,
                second_effectives=second_drug.Effectives
            };
            foreach(var second_conflict in second_conflicts)
            {
                conf_Drug.risk_Conflicts.Add(second_conflict);
            }
            return View(conf_Drug);
        }
        public ActionResult NoConflict()
        {
            var first_drug = (Drug_eff)Session["first_drug"];
            var second_drug = (Drug_eff)Session["second_drug"];
            List<risk_conflict> first_conflicts = first_drug.drug.risk_conflict.ToList();
            List<risk_conflict> second_conflicts = second_drug.drug.risk_conflict.ToList();
            conf_drug conf_Drug = new conf_drug
            {
                first_drug = first_drug.drug,
                first_effectives = first_drug.Effectives,
                risk_Conflicts = first_conflicts,
                second_drug = second_drug.drug,
                second_effectives = second_drug.Effectives
            };
            foreach (var second_conflict in second_conflicts)
            {
                conf_Drug.risk_Conflicts.Add(second_conflict);
            }
            return View(conf_Drug);
        }
        public ActionResult Logout(int? id)
        {
            return View();
        }
        [HttpPost]
        public ActionResult Logout()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
            Response.Cache.SetNoStore();
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();
            return RedirectToAction("Index", "user", new { area = "" });
        }
    }
}
