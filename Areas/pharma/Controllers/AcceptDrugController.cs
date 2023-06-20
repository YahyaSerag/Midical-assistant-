using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication8.Models;
using System.Data.Entity;
using System.Web.Security;

namespace WebApplication8.Areas.pharma.Controllers
{
    public class AcceptDrugController : Controller
    {
        DataContext db = new DataContext();
        // GET: pharma/AcceptDrug
        public ActionResult Index()
        {
            
            if (Session["phar_id"] == null)
            {
                return RedirectToAction("Index", "user", new { area = "" });
            }
            List<drug> drugs = db.drugs.Where(x => x.org == x.phar && x.risk_conflict.Count() != 0 && x.drug_effective.Count != 0).ToList();
            return View(drugs);
        }
        
        public ActionResult Search()
        {
            return View(db.drugs.Where(x => x.drug_effective.Count != 0 && x.risk_conflict.Count != 0 && x.phar != x.org).OrderByDescending(x => x.id));
        }
        [HttpPost]
        public ActionResult Search(string option, string search)
        {
            if (option == "TradeName")
            {
                return View(db.drugs.Where(x => x.TradeName.Contains(search) && x.org != x.phar && x.drug_effective.Count != 0 && x.risk_conflict.Count != 0));
            }
            else if (option == "ScientificName")
            {
                return View(db.drugs.Where(x => x.ScientificName.Contains(search) && x.org != x.phar && x.drug_effective.Count != 0 && x.risk_conflict.Count != 0));
            }
            else
            {
                return View(db.drugs.Where(x => x.drug_effective.Count != 0 && x.risk_conflict.Count != 0 && x.phar != x.org).OrderByDescending(x => x.id));
            }
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

        public ActionResult UpdateInfo()
        {
            int id = (int)Session["phar_id"];
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
                int id = (int)Session["phar_id"];
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
        // GET: pharma/AcceptDrug/Details/5
        public ActionResult Details(int id)
        {
            Session["drug_id"] = id;
            drug drug = db.drugs.Find(id);
            Session["org_id"] = drug.org;
            List<risk_conflict> risk_Conflicts = db.risk_conflict.Where(x => x.drug_id == id).ToList();    
            DrugDetails drugDetails = new DrugDetails
            {
                drug = drug,
                risk_Conflicts = risk_Conflicts
            };
            Session["drug"] = drugDetails;
            return View(drugDetails);
        }

        [HttpPost]
        public ActionResult AddEffective(string name)
        {

            if (string.IsNullOrEmpty(name) == false)
            {
                effective effective = new effective
                {
                    name = name.ToLower()
                };

                if (db.effectives.FirstOrDefault(x => x.name == effective.name) != null)
                {
                    TempData["effee"] = "This effective is exist";
                    if (TempData["z"] != null)
                    {
                        return RedirectToAction("AddRiskCon");
                    }
                    else
                    {
                        var risk_Conflict = (risk_conflict)Session["item"];
                        return RedirectToAction("editt", risk_Conflict);
                    }
                }
                db.effectives.Add(effective);
                db.SaveChanges();
                TempData["effee"] = "effective added successfully";
                if (TempData["z"] != null)
                {
                    return RedirectToAction("AddRiskCon");
                }
                else
                {
                    var risk_Conflict = (risk_conflict)Session["item"];
                    return RedirectToAction("editt", risk_Conflict);
                }
            }
            TempData["effee"] = "effective required";
            if (TempData["z"] != null)
            {
                return RedirectToAction("AddRiskCon");
            }
            else
            {
                var risk_Conflict = (risk_conflict)Session["item"];
                return RedirectToAction("editt", risk_Conflict);
            }
        }

        [HttpPost]
        public ActionResult AddRisk(string name)
        {

            if (string.IsNullOrEmpty(name) == false)
            {
                risk risk = new risk
                {
                    name = name.ToLower()
                };

                if (db.risks.FirstOrDefault(x => x.name == risk.name) != null)
                {
                    TempData["riskk"] = "This risk is exist";
                    if (TempData["z"] != null)
                    {
                        return RedirectToAction("AddRiskCon");
                    }
                    else
                    {
                        var risk_Conflict = (risk_conflict)Session["item"];
                        return RedirectToAction("editt", risk_Conflict);
                    }
                }
                db.risks.Add(risk);
                db.SaveChanges();
                TempData["riskk"] = "risk added successfully";
                if (TempData["z"] != null)
                {
                    return RedirectToAction("AddRiskCon");
                }
                else
                {
                    var risk_Conflict = (risk_conflict)Session["item"];
                    return RedirectToAction("editt", risk_Conflict);
                }
            }
            TempData["riskk"] = "risk required";
            if (TempData["z"] != null)
            {
                return RedirectToAction("AddRiskCon");
            }
            else
            {
                var risk_Conflict = (risk_conflict)Session["item"];
                return RedirectToAction("editt", risk_Conflict);
            }
        }

        public ActionResult editt(risk_conflict risk_Conflict)
        {
            Session["item"] = risk_Conflict;
            TempData["z"] = null;
            ViewBag.effective = new SelectList(db.effectives.OrderBy(x=>x.name), "id", "name", risk_Conflict.conflict_id);
            ViewBag.effective1 = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name", risk_Conflict.conflict_id_CON);
            ViewBag.risk_id = new SelectList(db.risks.OrderBy(x => x.name), "id", "name", risk_Conflict.risk_id);
            Session["risk_id"] = risk_Conflict.risk_id;
            Session["effective"] = risk_Conflict.conflict_id;
            Session["conflict"] = risk_Conflict.conflict_id_CON;
            return View(risk_Conflict);
        }
        [HttpPost]
        public ActionResult editt(risk_conflict risk_con, string effective, string effective1, string risk_id)
        {
            ViewBag.effective = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name", risk_con.conflict_id);
            ViewBag.effective1 = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name", risk_con.conflict_id_CON);
            ViewBag.risk_id = new SelectList(db.risks.OrderBy(x => x.name), "id", "name", risk_con.risk_id);
            if (effective != "" && effective1 != "" && risk_id != "")
            {
                int drug_id = (int)Session["drug_id"];
                int old_effective = (int)Session["effective"];
                int old_conflict = (int)Session["conflict"];
                int old_risk = (int)Session["risk_id"];
                int New_effective = Convert.ToInt32(effective);
                int New_conflict = Convert.ToInt32(effective1);
                int New_risk = Convert.ToInt32(risk_id);


                if (db.effectives.FirstOrDefault(x => x.id == New_effective).name == "none")
                {
                    ViewBag.message = "field effective should not be none";
                    return View(risk_con);
                }
                if (New_effective == New_conflict)
                {
                    ViewBag.message = "do not match between effective and conflict";
                    return View(risk_con);
                }
                
                if (db.effectives.FirstOrDefault(x => x.id == New_conflict).name == "none")
                {
                    if (db.risks.FirstOrDefault(x => x.id == New_risk).name != "none")
                    {
                        ViewBag.message = "do not match between conflict and risk";
                        return View(risk_con);
                    }
                }
                if (db.risks.FirstOrDefault(x => x.id == New_risk).name == "none")
                {
                    if (db.effectives.FirstOrDefault(x => x.id == New_conflict).name != "none")
                    {
                        ViewBag.message = "do not match between conflict and risk";
                        return View(risk_con);
                    }
                }
                if (db.conflicts.FirstOrDefault(x => x.effective_id == New_effective && x.effective_id_con == New_conflict) == null)
                {
                    conflict conflict = new conflict
                    {
                        effective_id = New_effective,
                        effective_id_con = New_conflict
                    };
                    db.conflicts.Add(conflict);
                    db.SaveChanges();
                }
                if (db.risk_conflict.FirstOrDefault(x => x.drug_id == drug_id && x.conflict_id == New_effective
                    && x.conflict_id_CON == New_conflict && x.risk_id == New_risk) != null)
                {
                    ViewBag.message = "this conflict is exist";
                    return View(risk_con);
                }
                risk_conflict s = db.risk_conflict.FirstOrDefault(x => x.conflict_id == old_effective &&
                x.conflict_id_CON == old_conflict && x.risk_id == old_risk && x.drug_id == drug_id);
                db.risk_conflict.Remove(s);
                db.SaveChanges();
                s.conflict_id = New_effective;
                s.conflict_id_CON = New_conflict;
                s.risk_id = New_risk;
                s.phar = (int)Session["phar_id"];
                s.org = null;

                db.risk_conflict.Add(s);
                db.SaveChanges();
                if (old_effective != New_effective)
                {
                    if (db.risk_conflict.Where(x => x.conflict_id == old_effective && x.drug_id == drug_id).Count() == 0)
                    {
                        var drug_effective = db.drug_effective.FirstOrDefault(x => x.drug_id == drug_id && x.effective_id == old_effective);
                        db.drug_effective.Remove(drug_effective);
                        db.SaveChanges();
                    }

                    if (db.drug_effective.FirstOrDefault(x => x.drug_id == drug_id && x.effective_id == New_effective) == null)
                    {
                        drug_effective drug_Effective = new drug_effective
                        {
                            drug_id = drug_id,
                            effective_id = New_effective
                        };
                        db.drug_effective.Add(drug_Effective);
                        db.SaveChanges();
                    }
                }
                if (db.risk_conflict.Where(x => x.conflict_id == old_effective && x.conflict_id_CON == old_conflict).Count() == 0)
                {
                    var con = db.conflicts.FirstOrDefault(x => x.effective_id == old_effective && x.effective_id_con == old_conflict);
                    db.conflicts.Remove(con);
                    db.SaveChanges();
                }
                return RedirectToAction("Details", new { id = (int)Session["drug_id"] });
            }
            ViewBag.message = "fields not complete";
            return View(risk_con);
        }

        public ActionResult Edit(int id)
        {
            drug drug = db.drugs.Find(id);
            return View(drug);   
        }
        [HttpPost]
        public ActionResult Edit(drug drug)
        {
            if (ModelState.IsValid)
            {
                drug.org = (int)Session["org_id"];
                drug.phar = (int)Session["phar_id"];
                db.Entry(drug).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = drug.id});
            }
            return View(drug);
        }
        public ActionResult AddRiskCon()
        {
            TempData["z"] = true;
            ViewBag.effective = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
            ViewBag.effective1 = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
            ViewBag.risk_id = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
            return View();
        }

        [HttpPost]
        public ActionResult AddRiskCon(risk_conflict risk_con, string effective, string effective1, string risk_id)
        {
            ViewBag.effective = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
            ViewBag.effective1 = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
            ViewBag.risk_id = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
            if (effective != "" && effective1 != "" && risk_id != "")
            {
                int New_effective = Convert.ToInt32(effective);
                int New_conflict = Convert.ToInt32(effective1);
                int New_risk = Convert.ToInt32(risk_id);
                int drug_id = (int)Session["drug_id"];
                int phar_id = (int)Session["phar_id"];

                if (db.effectives.FirstOrDefault(x => x.id == New_effective).name == "none")
                {
                    ViewBag.message = "field effective should not be none";
                    return View(risk_con);
                }
                if (New_effective == New_conflict)
                {
                    ViewBag.message = "do not match between effective and conflict";
                    return View(risk_con);
                }
                
                if (db.effectives.FirstOrDefault(x => x.id == New_conflict).name == "none")
                {
                    if (db.risks.FirstOrDefault(x => x.id == New_risk).name != "none")
                    {
                        ViewBag.message = "do not match between conflict and risk";
                        return View(risk_con);
                    }
                }
                if (db.risks.FirstOrDefault(x => x.id == New_risk).name == "none")
                {
                    if (db.effectives.FirstOrDefault(x => x.id == New_conflict).name != "none")
                    {
                        ViewBag.message = "do not match between conflict and risk";
                        return View(risk_con);
                    }
                }
                if (db.drug_effective.FirstOrDefault(x => x.drug_id == drug_id && x.effective_id == New_effective) == null)
                {
                    drug_effective drug_Effective = new drug_effective
                    {
                        drug_id = drug_id,
                        effective_id = New_effective
                    };
                    db.drug_effective.Add(drug_Effective);
                    db.SaveChanges();
                }
                if (db.conflicts.FirstOrDefault(x => x.effective_id == New_effective && x.effective_id_con == New_conflict) == null)
                {
                    conflict conflict = new conflict
                    {
                        effective_id = New_effective,
                        effective_id_con = New_conflict
                    };
                    db.conflicts.Add(conflict);
                    db.SaveChanges();
                }
                if (db.risk_conflict.FirstOrDefault(x => x.conflict_id == New_effective && x.conflict_id_CON == New_conflict
                    && x.drug_id == drug_id && x.risk_id == New_risk) != null)
                {
                    ViewBag.message = "this conflict is exist";
                    return View(risk_con);
                }

                risk_conflict risk_Conflict = new risk_conflict
                {
                    conflict_id = New_effective,
                    conflict_id_CON = New_conflict,
                    risk_id = New_risk,
                    drug_id = drug_id,
                    phar = phar_id,
                    org = null
                };
                db.risk_conflict.Add(risk_Conflict);
                db.SaveChanges();

                return RedirectToAction("Details", new { id = drug_id });
            }
            ViewBag.message = "fields not complete";
            return View(risk_con);
        }

        public ActionResult DeleteRiskCon(risk_conflict risk_Conflict)
        {
            risk_Conflict.conflict = db.conflicts.FirstOrDefault(x => x.effective_id == risk_Conflict.conflict_id
                                     && x.effective_id_con == risk_Conflict.conflict_id_CON);
            risk_Conflict.risk = db.risks.Find(risk_Conflict.risk_id);
            return View(risk_Conflict);
        }
        [HttpPost,ActionName("DeleteRiskCon")]
        public ActionResult DeleteRiskConConfirm(risk_conflict risk_Conflict)
        {
            risk_conflict risk_con = db.risk_conflict.FirstOrDefault(x => x.drug_id == risk_Conflict.drug_id &&
            x.conflict_id == risk_Conflict.conflict_id && x.conflict_id_CON == risk_Conflict.conflict_id_CON && x.risk_id == risk_Conflict.risk_id);
            db.risk_conflict.Remove(risk_con);
            db.SaveChanges();
            if (db.risk_conflict.Where(x => x.conflict_id == risk_Conflict.conflict_id 
                && x.conflict_id_CON == risk_Conflict.conflict_id_CON).Count() == 0)
            {
                conflict conflict = db.conflicts.FirstOrDefault(x => x.effective_id == risk_Conflict.conflict_id
                      && x.effective_id_con == risk_Conflict.conflict_id_CON);
                db.conflicts.Remove(conflict);
                db.SaveChanges();
            }
            if (db.risk_conflict.Where(x => x.drug_id == risk_Conflict.drug_id
                && x.conflict_id == risk_Conflict.conflict_id).Count() == 0)
            {
                drug_effective drug_Effective = db.drug_effective.FirstOrDefault(x => x.effective_id == risk_Conflict.conflict_id
                      && x.drug_id == risk_Conflict.drug_id);
                db.drug_effective.Remove(drug_Effective);
                db.SaveChanges();
            }
            return RedirectToAction("Details", new { id = (int)Session["drug_id"] });
        }

        public ActionResult Accept(int id)
        {   
            drug drug = db.drugs.Find(id);
            drug.phar = (int)Session["phar_id"];
            db.Entry(drug).State = EntityState.Modified;
            db.SaveChanges();
            
            List<risk_conflict> risk_Conflicts = db.risk_conflict.Where(x => x.drug_id == id).ToList();
            foreach(var risk_Conflict in risk_Conflicts)
            {
                risk_Conflict.phar = (int)Session["phar_id"];
                db.Entry(risk_Conflict).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
