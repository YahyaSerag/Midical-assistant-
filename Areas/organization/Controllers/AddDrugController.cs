using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebApplication8.Models;

namespace WebApplication8.Areas.organization.Controllers
{
    public class AddDrugController : Controller
    {
        DataContext db = new DataContext();
        
        public ActionResult Index()
        {
            
            if (Session["org_id"] == null)
            {
                return RedirectToAction("Index", "user", new { area = "" });
            }
            int id = (int)Session["org_id"];
            return View(db.drugs.Where(x => x.org == id && x.org == x.phar
            && x.drug_effective.Count() != 0 && x.risk_conflict.Count() != 0).OrderByDescending(x => x.id));
        }
        public ActionResult UpdateInfo()
        {
            int id = (int)Session["org_id"];
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
                int id = (int)Session["org_id"];
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
        public ActionResult AcceptedDrug()
        {
            int id = (int)Session["org_id"];
            return View(db.drugs.Where(x => x.org == id && x.org != x.phar).OrderByDescending(x => x.id));
        }

        public ActionResult NotCompleteRisk()
        {
            int id = (int)Session["org_id"];
            return View(db.drugs.Where(x => x.risk_conflict.Count == 0 && x.org == id && x.drug_effective.Count != 0));
        }


        public ActionResult NotCompleteEffe()
        {
            int id = (int)Session["org_id"];
            return View(db.drugs.Where(x => x.drug_effective.Count == 0 && x.org == id));
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

        public ActionResult Details(int id)
        {
            Session["drug_id"] = id;
            drug drug = db.drugs.Find(id);
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
                return RedirectToAction("editt",  risk_Conflict );
            }
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
                int org_id = (int)Session["org_id"];
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
                    phar = org_id,
                    org = org_id
                };
                db.risk_conflict.Add(risk_Conflict);
                db.SaveChanges();
                
                return RedirectToAction("Details", new { id = drug_id });
            }
            ViewBag.message = "fields not complete";
            return View(risk_con);
        }

        public ActionResult editt(risk_conflict risk_Conflict)
        {
            Session["item"] = risk_Conflict;
            TempData["z"] = null;
            ViewBag.effective = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name", risk_Conflict.conflict_id);
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
                s.phar = (int)Session["org_id"];
                s.org = (int)Session["org_id"];
                
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

        public ActionResult DeleteRiskCon(risk_conflict risk_Conflict)
        {
            risk_Conflict.conflict = db.conflicts.FirstOrDefault(x => x.effective_id == risk_Conflict.conflict_id
                                     && x.effective_id_con == risk_Conflict.conflict_id_CON);
            risk_Conflict.risk = db.risks.Find(risk_Conflict.risk_id);
            return View(risk_Conflict);
        }
        [HttpPost, ActionName("DeleteRiskCon")]
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
                drug.phar = (int)Session["org_id"];
                db.Entry(drug).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = drug.id });
            }
            return View(drug);
        }

        
        public ActionResult AddCon(int id)
        {
            Session["drug_id"] = id;
            List<drug_effective> drug_Effectives = db.drug_effective.Where(x => x.drug_id == id).ToList();
            List<effective> effes = new List<effective>();
            List<AddEffe> addEffes = new List<AddEffe>();
            foreach(var x in drug_Effectives)
            {
                AddEffe addEffe = new AddEffe
                {
                   old_effe=x.effective,
                    No_con = 1
                };
                addEffes.Add(addEffe);
                effes.Add(x.effective);
            }
            Session["effes"] = effes;
            return View(addEffes);
        }
        [HttpPost]
        public ActionResult AddCon(List<AddEffe> addEffes)
        {
            var effes = Session["effes"] as List<effective>;
            int c = 0;
            var No_cons = new List<int>();
            foreach (var x in addEffes)
            {
                x.old_effe = effes[c];
                c++;
                No_cons.Add(x.No_con);
            }
            if (ModelState.IsValid)
            {
                Session["No_con"] = No_cons;
                return RedirectToAction("AddConflict");
            }
            else
            {
                return View(addEffes);
            }
        }

        public ActionResult AddEffecttive(int id)
        {
            drug drug = db.drugs.Find(id);
            Session["drug_id"] = drug.id;
            List<drug_effective> drug_Effectives = db.drug_effective.Where(x => x.drug_id == id).ToList();
            List<effective> effectives = new List<effective>();
            foreach(var x  in drug_Effectives)
            {
                effectives.Add(x.effective);
            }
            Session["effes"] = effectives;
            AddDrug addDrug = new AddDrug
            {
                drug=drug,
                No_effe=1
            };
            return View(addDrug);
        }


        [HttpPost]
        public ActionResult AddEffecttive(AddDrug addDrug)
        {
            return RedirectToAction("AddEffe",new {addDrug.No_effe });
        }
        
        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Create(AddDrug addDrug)
        {
            if(ModelState.IsValid)
            {
                drug drug = new drug
                {
                    TradeName = addDrug.drug.TradeName.ToLower(),
                    ScientificName = addDrug.drug.ScientificName.ToLower(),
                    prescription = addDrug.drug.prescription,
                    SideEffect = addDrug.drug.SideEffect,
                    phar = (int)Session["org_id"],
                    org = (int)Session["org_id"]
                };
                db.drugs.Add(drug);
                db.SaveChanges();
                Session["drug_id"] = drug.id;
                return RedirectToAction("AddEffe",new { addDrug.No_effe});
            }
            return View(addDrug);
        }

        
        public ActionResult AddEffe(int No_effe)
        {
            Session["No_effe"] = No_effe;
            ViewBag.effe = new SelectList(db.effectives.Where(x=>x.name!="none").OrderBy(x=>x.name), "id", "name");
            List<AddEffe> addEffes = new List<AddEffe>();
            for (var i = 1; i <= No_effe; i++) 
            {
                AddEffe effe = new AddEffe
                {
                    No_con = 1
                };
                addEffes.Add(effe);
            }
            return View(addEffes);
        }

        
        [HttpPost]
        public ActionResult AddEffe(List<AddEffe> effectives)
        {
            if (ModelState.IsValid)
            {
                List<effective> effes = new List<effective>();
                List<int> No_con = new List<int>();
                int c = 0;
                foreach (var x in effectives)
                {
                    if (db.effectives.Find(Convert.ToInt32(x.old_effe.name)) != null && string.IsNullOrEmpty(x.New_effe.name) == true)
                    {
                        effes.Add(db.effectives.Find(Convert.ToInt32(x.old_effe.name)));
                    }
                    else if(db.effectives.Find(Convert.ToInt32(x.old_effe.name)) == null && string.IsNullOrEmpty(x.New_effe.name)==false)
                    {
                        if (db.effectives.FirstOrDefault(z => z.name == x.New_effe.name.ToLower()) != null)
                        {
                            if (x.New_effe.name.ToLower() == "none")
                            {
                                ViewBag.c = c;
                                ViewBag.error = "please enter effective or choose one";
                                ViewBag.effe = new SelectList(db.effectives.Where(z => z.name != "none").OrderBy(z => z.name), "id", "name");
                                return View(effectives);
                            }
                            effes.Add(db.effectives.FirstOrDefault(z => z.name == x.New_effe.name.ToLower()));
                        }
                        else
                        {
                            effective effe = new effective
                            {
                                name = x.New_effe.name.ToLower()
                            };
                            db.effectives.Add(effe);
                            db.SaveChanges();
                            effes.Add(effe);
                        }
                    }
                    else if(db.effectives.Find(Convert.ToInt32(x.old_effe.name)) == null && string.IsNullOrEmpty(x.New_effe.name) == true)
                    {
                        ViewBag.c = c;
                        ViewBag.error = "please enter effective or choose one";
                        ViewBag.effe = new SelectList(db.effectives.Where(z => z.name != "none").OrderBy(z => z.name), "id", "name");
                        return View(effectives);
                    }
                    else if(db.effectives.Find(Convert.ToInt32(x.old_effe.name)) != null && string.IsNullOrEmpty(x.New_effe.name) == false)
                    {
                        ViewBag.c = c;
                        ViewBag.error = "please enter effective or choose one";
                        ViewBag.effe = new SelectList(db.effectives.Where(z => z.name != "none").OrderBy(z => z.name), "id", "name");
                        return View(effectives);
                    }
                    No_con.Add(x.No_con);
                    c++;
                }
                
                foreach (var effective in effes)
                {
                    drug_effective drug_Effective = new drug_effective
                    {
                        drug_id = (int)Session["drug_id"],
                        effective_id = effective.id
                    };
                    db.drug_effective.Add(drug_Effective);
                    db.SaveChanges();
                }
                Session["effes"] = effes;
                Session["No_con"] = No_con;
                return RedirectToAction("AddConflict");
            }
            ViewBag.effe = new SelectList(db.effectives.Where(z => z.name != "none").OrderBy(z => z.name), "id", "name");
            return View(effectives);
        }

        
        public ActionResult AddConflict()
        {
            List<int> No_con = Session["No_con"] as List<int>;
            List<effective> effe = Session["effes"] as List<effective>;
            List<Risk_con> risk_Conflicts = new List<Risk_con>();
            var c = 0;
            foreach(var n in No_con)
            {
                for (var i = 0; i < n; i++) 
                {
                    Risk_con risk_Conflict = new Risk_con
                    {
                        effective = effe[c]
                    };
                    risk_Conflicts.Add(risk_Conflict);
                }
                c++;
            }
            ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
            ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
            return View(risk_Conflicts);
        }

        
        [HttpPost]
        public ActionResult AddConflict(List<Risk_con> risk_Conflicts)
        {
            if (ModelState.IsValid)
            {
                List<int> No_con = Session["No_con"] as List<int>;
                List<effective> effe = Session["effes"] as List<effective>;
                int c = 0;
                int v = 0;
                foreach (var i in No_con)
                {
                    for (int x = 0; x < i; x++)
                    {
                        risk_Conflicts[v].effective = effe[c];
                        v++;
                    }
                    c++;
                }


                List<effective> conflicts = new List<effective>();
                List<risk> risks = new List<risk>();
                List<risk_conflict> risk_con = new List<risk_conflict>();
                foreach (var x in risk_Conflicts)
                {
                    risk_conflict risk_Conflict = new risk_conflict
                    {
                        conflict_id = x.effective.id
                    };
                    risk_con.Add(risk_Conflict);
                }

                int p = 0;
                int o = 0;

                foreach (var risk_conflict in risk_Conflicts)
                {
                    if (db.effectives.Find(Convert.ToInt32(risk_conflict.old_conflict.name)) != null && string.IsNullOrEmpty(risk_conflict.New_conflict.name) == true)
                    {
                        if (db.effectives.Find(Convert.ToInt32(risk_conflict.old_conflict.name)).name == "none")
                        {
                            if ((db.risks.Find(Convert.ToInt32(risk_conflict.old_risk.name)) != null &&
                                string.IsNullOrEmpty(risk_conflict.New_risk.name) == true))
                            {
                                if (db.risks.Find(Convert.ToInt32(risk_conflict.old_risk.name)).name != "none")
                                {
                                    ViewBag.o = o;
                                    ViewBag.errorr = "do not match between effective and risk";
                                    ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                                    ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                                    return View(risk_Conflicts);
                                }
                            }
                            else if ((string.IsNullOrEmpty(risk_conflict.New_risk.name) == false &&db.risks.Find(Convert.ToInt32(risk_conflict.old_risk.name)) == null))
                            {
                                if (risk_conflict.New_risk.name.ToLower() != "none")
                                {
                                    ViewBag.o = o;
                                    ViewBag.errorr = "do not match between effective and risk";
                                    ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                                    ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                                    return View(risk_Conflicts);
                                }
                            }



                            else if ((db.risks.Find(Convert.ToInt32(risk_conflict.old_risk.name)) != null) &&string.IsNullOrEmpty(risk_conflict.New_risk.name) == false)
                            {
                                ViewBag.o = o;
                                ViewBag.errorr = "please enter conflict or choose one";
                                ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                                ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                                return View(risk_Conflicts);
                            }
                            else if ((db.risks.Find(Convert.ToInt32(risk_conflict.old_risk.name)) == null) &&string.IsNullOrEmpty(risk_conflict.New_risk.name) == true)
                            {
                                ViewBag.o = o;
                                ViewBag.errorr = "please enter conflict or choose one";
                                ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                                ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                                return View(risk_Conflicts);
                            }
                        }
                        conflicts.Add(db.effectives.Find(Convert.ToInt32(risk_conflict.old_conflict.name)));
                    }
                    else if (db.effectives.Find(Convert.ToInt32(risk_conflict.old_conflict.name)) == null &&string.IsNullOrEmpty(risk_conflict.New_conflict.name) == false)
                    {
                        if (db.effectives.FirstOrDefault(z => z.name == risk_conflict.New_conflict.name.ToLower()) != null)
                        {
                            if (risk_conflict.New_conflict.name.ToLower() == "none")
                            {
                                if ((db.risks.Find(Convert.ToInt32(risk_conflict.old_risk.name)) != null && string.IsNullOrEmpty(risk_conflict.New_risk.name) == true))
                                {
                                    if (db.risks.Find(Convert.ToInt32(risk_conflict.old_risk.name)).name != "none")
                                    {
                                        ViewBag.o = o;
                                        ViewBag.errorr = "do not match between effective and risk";
                                        ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                                        ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                                        return View(risk_Conflicts);
                                    }
                                }
                                else if ((string.IsNullOrEmpty(risk_conflict.New_risk.name) == false && db.risks.Find(Convert.ToInt32(risk_conflict.old_risk.name)) == null))
                                {
                                    if (risk_conflict.New_risk.name.ToLower() != "none")
                                    {
                                        ViewBag.o = o;
                                        ViewBag.errorr = "do not match between effective and risk";
                                        ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                                        ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                                        return View(risk_Conflicts);
                                    }
                                }



                                else if ((db.risks.Find(Convert.ToInt32(risk_conflict.old_risk.name)) != null) && string.IsNullOrEmpty(risk_conflict.New_risk.name) == false)
                                {
                                    ViewBag.o = o;
                                    ViewBag.errorr = "please enter conflict or choose one";
                                    ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                                    ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                                    return View(risk_Conflicts);
                                }
                                else if ((db.risks.Find(Convert.ToInt32(risk_conflict.old_risk.name)) == null) && string.IsNullOrEmpty(risk_conflict.New_risk.name) == true)
                                {
                                    ViewBag.o = o;
                                    ViewBag.errorr = "please enter conflict or choose one";
                                    ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                                    ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                                    return View(risk_Conflicts);
                                }
                            }

                            conflicts.Add(db.effectives.FirstOrDefault(z => z.name == risk_conflict.New_conflict.name.ToLower()));
                        }
                        else
                        {
                            effective effect = new effective
                            {
                                name = risk_conflict.New_conflict.name.ToLower()
                            };
                            db.effectives.Add(effect);
                            db.SaveChanges();
                            conflicts.Add(effect);
                        }
                    }
                 
                    else if (db.effectives.Find(Convert.ToInt32(risk_conflict.old_conflict.name)) == null && string.IsNullOrEmpty(risk_conflict.New_conflict.name) == true)
                    {
                        ViewBag.p = p;
                        ViewBag.error = "please enter conflict or choose one";
                        ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                        ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                        return View(risk_Conflicts);
                    }
                    else if (db.effectives.Find(Convert.ToInt32(risk_conflict.old_conflict.name)) != null && string.IsNullOrEmpty(risk_conflict.New_conflict.name) == false)
                    {
                        ViewBag.p = p;
                        ViewBag.error = "please enter conflict or choose one";
                        ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                        ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                        return View(risk_Conflicts);
                    }



                    

                    if (db.risks.Find(Convert.ToInt32(risk_conflict.old_risk.name)) != null && string.IsNullOrEmpty(risk_conflict.New_risk.name) == true)
                    {
                        if (db.risks.Find(Convert.ToInt32(risk_conflict.old_risk.name)).name == "none")
                        {
                            if ((db.effectives.Find(Convert.ToInt32(risk_conflict.old_conflict.name)) != null && string.IsNullOrEmpty(risk_conflict.New_conflict.name) == true)) 
                            {
                                if (db.effectives.Find(Convert.ToInt32(risk_conflict.old_conflict.name)).name != "none")
                                {
                                    ViewBag.p = p;
                                    ViewBag.error = "do not match between effective and risk";
                                    ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                                    ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                                    return View(risk_Conflicts);
                                }
                            }
                            else if ((string.IsNullOrEmpty(risk_conflict.New_conflict.name) == false && db.risks.Find(Convert.ToInt32(risk_conflict.old_conflict.name)) == null))
                            {
                                if (risk_conflict.New_conflict.name.ToLower() != "none")
                                {
                                    ViewBag.p = p;
                                    ViewBag.error = "do not match between effective and risk";
                                    ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                                    ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                                    return View(risk_Conflicts);
                                }
                            }



                            else if ((db.effectives.Find(Convert.ToInt32(risk_conflict.old_conflict.name)) != null) && string.IsNullOrEmpty(risk_conflict.New_conflict.name) == false)
                            {
                                ViewBag.p = p;
                                ViewBag.error = "please enter conflict or choose one";
                                ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                                ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                                return View(risk_Conflicts);
                            }
                            else if ((db.effectives.Find(Convert.ToInt32(risk_conflict.old_conflict.name)) == null) && string.IsNullOrEmpty(risk_conflict.New_conflict.name) == true)
                            {
                                ViewBag.p = p;
                                ViewBag.error = "please enter conflict or choose one";
                                ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                                ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                                return View(risk_Conflicts);
                            }
                        }
                        risks.Add(db.risks.Find(Convert.ToInt32(risk_conflict.old_risk.name)));
                    }
                    else if (db.risks.Find(Convert.ToInt32(risk_conflict.old_risk.name)) == null && string.IsNullOrEmpty(risk_conflict.New_risk.name) == false)
                    {
                        if (db.risks.FirstOrDefault(z => z.name == risk_conflict.New_risk.name.ToLower()) != null)
                        {
                            if (risk_conflict.New_risk.name.ToLower() == "none")
                            {
                                if ((db.effectives.Find(Convert.ToInt32(risk_conflict.old_conflict.name)) != null && string.IsNullOrEmpty(risk_conflict.New_conflict.name) == true))
                                {
                                    if (db.effectives.Find(Convert.ToInt32(risk_conflict.old_conflict.name)).name != "none")
                                    {
                                        ViewBag.p = p;
                                        ViewBag.error = "do not match between effective and risk";
                                        ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                                        ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                                        return View(risk_Conflicts);
                                    }
                                }
                                else if ((string.IsNullOrEmpty(risk_conflict.New_conflict.name) == false && db.risks.Find(Convert.ToInt32(risk_conflict.old_conflict.name)) == null))
                                {
                                    if (risk_conflict.New_conflict.name.ToLower() != "none")
                                    {
                                        ViewBag.p = p;
                                        ViewBag.error = "do not match between effective and risk";
                                        ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                                        ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                                        return View(risk_Conflicts);
                                    }
                                }



                                else if ((db.effectives.Find(Convert.ToInt32(risk_conflict.old_conflict.name)) != null) && string.IsNullOrEmpty(risk_conflict.New_conflict.name) == false)
                                {
                                    ViewBag.p = p;
                                    ViewBag.error = "please enter conflict or choose one";
                                    ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                                    ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                                    return View(risk_Conflicts);
                                }
                                else if ((db.effectives.Find(Convert.ToInt32(risk_conflict.old_conflict.name)) == null) && string.IsNullOrEmpty(risk_conflict.New_conflict.name) == true)
                                {
                                    ViewBag.p = p;
                                    ViewBag.error = "please enter conflict or choose one";
                                    ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                                    ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                                    return View(risk_Conflicts);
                                }
                            }

                            risks.Add(db.risks.FirstOrDefault(z => z.name == risk_conflict.New_risk.name.ToLower()));
                        }
                        else
                        {
                            risk riskk = new risk
                            {
                                name = risk_conflict.New_risk.name.ToLower()
                            };
                            db.risks.Add(riskk);
                            db.SaveChanges();
                            risks.Add(riskk);
                        }
                    }
                    else if (db.risks.Find(Convert.ToInt32(risk_conflict.old_risk.name)) == null && string.IsNullOrEmpty(risk_conflict.New_risk.name) == true)
                    {
                        ViewBag.o = o;
                        ViewBag.errorr = "please enter conflict or choose one";
                        ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                        ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                        return View(risk_Conflicts);
                    }
                    else if (db.risks.Find(Convert.ToInt32(risk_conflict.old_risk.name)) != null && string.IsNullOrEmpty(risk_conflict.New_risk.name) == false)
                    {
                        ViewBag.o = o;
                        ViewBag.errorr = "please enter conflict or choose one";
                        ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
                        ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
                        return View(risk_Conflicts);
                    }
                    p++;
                    o++;
                }
                for(int i = 0; i < risk_con.Count; i++)
                {
                    risk_con[i].conflict_id_CON = conflicts[i].id;
                    int w = risk_con[i].conflict_id;
                    int q = risk_con[i].conflict_id_CON;
                    risk_con[i].risk_id = risks[i].id;
                    risk_con[i].org = (int)Session["org_id"];
                    risk_con[i].phar= (int)Session["org_id"];
                    risk_con[i].drug_id= (int)Session["drug_id"];
                    if (db.conflicts.FirstOrDefault(x => x.effective_id == w && x.effective_id_con == q) == null) 
                    {
                        conflict conflict = new conflict
                        {
                            effective_id = risk_con[i].conflict_id,
                            effective_id_con = risk_con[i].conflict_id_CON,
                        };
                        db.conflicts.Add(conflict);
                        db.SaveChanges();
                    }
                    db.risk_conflict.Add(risk_con[i]);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }

            ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
            ViewBag.risk = new SelectList(db.risks.OrderBy(x => x.name), "id", "name");
            return View(risk_Conflicts);
            
        }
    }
}


