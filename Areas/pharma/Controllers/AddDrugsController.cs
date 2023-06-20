using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication8.Models;

namespace WebApplication8.Areas.pharma.Controllers
{
    public class AddDrugsController : Controller
    {
        private DataContext db = new DataContext();
        // GET: pharma/AddDrugs
        public ActionResult Index()
        {
            return View(db.waitings);
        }

        // GET: pharma/AddDrugs/Details/5
        public ActionResult Details(int id)
        {
            Session["wait_id"] = id;
            var wait = db.waitings.Find(id);
            return RedirectToAction("AddWait",new {wait.name });
        }
        public ActionResult AddWait(string name)
        {
            drug drug = new drug
            {
                TradeName = name
            };
            AddDrug addDrug = new AddDrug
            {
                drug = drug,
                No_effe=1

            };
            return View(addDrug);
        }

        [HttpPost]
        public ActionResult AddWait(AddDrug addDrug)
        {
            if (ModelState.IsValid)
            {
                drug drug = new drug
                {
                    TradeName=addDrug.drug.TradeName,
                    ScientificName = addDrug.drug.ScientificName,
                    prescription = addDrug.drug.prescription,
                    SideEffect = addDrug.drug.SideEffect,
                    phar = (int)Session["phar_id"]
                };
                db.drugs.Add(drug);
                db.SaveChanges();
                Session["drug_id"] = drug.id;
                waiting wait = db.waitings.Find((int)Session["wait_id"]);
                db.waitings.Remove(wait);
                db.SaveChanges();
                return RedirectToAction("AddEffe", new { addDrug.No_effe });
            }
            else
            {
                return View(addDrug);
            }
        }

        public ActionResult NotCompleteRisk()
        {
            var z = db.drugs.Where(x => x.risk_conflict.Count == 0 && x.org == null && x.drug_effective.Count != 0).ToList();
            return View(z);
        }


        public ActionResult NotCompleteEffe()
        {
            var z = db.drugs.Where(x => x.drug_effective.Count == 0 && x.org == null).ToList();
            return View(z);
        }


        public ActionResult AddCon(int id)
        {
            Session["drug_id"] = id;
            List<drug_effective> drug_Effectives = db.drug_effective.Where(x => x.drug_id == id).ToList();
            List<effective> effes = new List<effective>();
            List<AddEffe> addEffes = new List<AddEffe>();
            foreach (var x in drug_Effectives)
            {
                AddEffe addEffe = new AddEffe
                {
                    old_effe = x.effective,
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
            foreach (var x in drug_Effectives)
            {
                effectives.Add(x.effective);
            }
            Session["effes"] = effectives;
            AddDrug addDrug = new AddDrug
            {
                drug = drug,
                No_effe = 1
            };
            return View(addDrug);
        }


        [HttpPost]
        public ActionResult AddEffecttive(AddDrug addDrug)
        {
            return RedirectToAction("AddEffe", new { addDrug.No_effe });
        }

      

        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Create(AddDrug addDrug)
        {
            if (ModelState.IsValid)
            {
                drug drug = new drug
                {
                    TradeName = addDrug.drug.TradeName.ToLower(),
                    ScientificName = addDrug.drug.ScientificName.ToLower(),
                    prescription = addDrug.drug.prescription,
                    SideEffect = addDrug.drug.SideEffect,
                    phar = (int)Session["phar_id"]
                };
                db.drugs.Add(drug);
                db.SaveChanges();
                Session["drug_id"] = drug.id;
                return RedirectToAction("AddEffe", new { addDrug.No_effe });
            }
            else
            {
                return View(addDrug);
            }
        }


        public ActionResult AddEffe(int No_effe)
        {
            Session["No_effe"] = No_effe;
            ViewBag.effe = new SelectList(db.effectives.OrderBy(x => x.name), "id", "name");
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
                        if (db.effectives.Find(Convert.ToInt32(x.old_effe.name)).name == "none")
                        {
                            ViewBag.c = c;
                            ViewBag.error = "please enter effective or choose one";
                            ViewBag.effe = new SelectList(db.effectives.OrderBy(z => z.name), "id", "name");
                            return View(effectives);
                        }
                        effes.Add(db.effectives.Find(Convert.ToInt32(x.old_effe.name)));
                    }
                    else if (db.effectives.Find(Convert.ToInt32(x.old_effe.name)) == null && string.IsNullOrEmpty(x.New_effe.name) == false)
                    {
                        if (db.effectives.FirstOrDefault(z => z.name == x.New_effe.name.ToLower()) != null)
                        {
                            if (x.New_effe.name.ToLower() == "none")
                            {
                                ViewBag.c = c;
                                ViewBag.error = "please enter effective or choose one";
                                ViewBag.effe = new SelectList(db.effectives.OrderBy(z => z.name), "id", "name");
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
                    else if (db.effectives.Find(Convert.ToInt32(x.old_effe.name)) == null && string.IsNullOrEmpty(x.New_effe.name) == true)
                    {
                        ViewBag.c = c;
                        ViewBag.error = "please enter effective or choose one";
                        ViewBag.effe = new SelectList(db.effectives.OrderBy(z => z.name), "id", "name");
                        return View(effectives);
                    }
                    else if (db.effectives.Find(Convert.ToInt32(x.old_effe.name)) != null && string.IsNullOrEmpty(x.New_effe.name) == false)
                    {
                        ViewBag.c = c;
                        ViewBag.error = "please enter effective or choose one";
                        ViewBag.effe = new SelectList(db.effectives.OrderBy(z => z.name), "id", "name");
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
            ViewBag.effe = new SelectList(db.effectives.OrderBy(z => z.name), "id", "name");
            return View(effectives);
        }


        public ActionResult AddConflict()
        {
            List<int> No_con = Session["No_con"] as List<int>;
            List<effective> effe = Session["effes"] as List<effective>;
            List<Risk_con> risk_Conflicts = new List<Risk_con>();
            var c = 0;
            foreach (var n in No_con)
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
                        conflicts.Add(db.effectives.Find(Convert.ToInt32(risk_conflict.old_conflict.name)));
                    }
                    else if (db.effectives.Find(Convert.ToInt32(risk_conflict.old_conflict.name)) == null && string.IsNullOrEmpty(risk_conflict.New_conflict.name) == false)
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
                for (int i = 0; i < risk_con.Count; i++)
                {
                    risk_con[i].conflict_id_CON = conflicts[i].id;
                    int w = risk_con[i].conflict_id;
                    int q = risk_con[i].conflict_id_CON;
                    risk_con[i].risk_id = risks[i].id;
                    risk_con[i].phar = (int)Session["phar_id"];
                    risk_con[i].drug_id = (int)Session["drug_id"];
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

            ViewBag.effe = new SelectList(db.effectives, "id", "name");
            ViewBag.risk = new SelectList(db.risks, "id", "name");
            return View(risk_Conflicts);

        }
    

        // GET: pharma/AddDrugs/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: pharma/AddDrugs/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: pharma/AddDrugs/Delete/5
        public ActionResult Delete(int id)
        {
            var wait = db.waitings.Find(id);
            db.waitings.Remove(wait);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        
    }
}
