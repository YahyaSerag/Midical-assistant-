using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication8.Models;
using System.Data.Entity;
using System.Web.Security;

namespace WebApplication8.Areas.admin.Controllers
{
    public class ControlController : Controller
    {
        DataContext db = new DataContext();
        public ActionResult Index()
        {
            
            if (Session["admin_id"] == null)
            {
                return RedirectToAction("Index", "user",new {area="" });
            }
            
            return View(db.users.Where(x=>x.accept==accept.NotAccept));
        }

        public ActionResult Enable_accounts()
        {
            int id = (int)Session["admin_id"];
            var users = db.users.Where(x => x.state == state.enable && x.accept == accept.Accept && x.role != role.doctor && x.id != id).ToList();
            return View(users);
        }
        public ActionResult Disable_accounts()
        {
            var users = db.users.Where(x => x.state == state.disable && x.accept == accept.Accept && x.role != role.doctor).ToList();
            return View(users);
        }

        public ActionResult Disable(int id)
        {
            var user = db.users.Find(id);
            user.state = state.disable;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Enable_accounts");
        }

        public ActionResult enable(int id)
        {
            var user = db.users.Find(id);
            user.state = state.enable;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Disable_accounts");
        }
        public ActionResult Accept(int id)
        {
            user user = db.users.Find(id);
            return View(user);
        }
        [HttpPost, ActionName("Accept")]
        public ActionResult Acceptt(int id)
        {
            user user = db.users.Find(id);
            user.accept = accept.Accept;
            db.Entry(user).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Deny(int id)
        {
            user user = db.users.Find(id);
            return View(user);
        }
        [HttpPost, ActionName("Deny")]
        public ActionResult Denyy(int id)
        {
            user user = db.users.Find(id);
            db.users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult UpdateInfo()
        {
            int id = (int)Session["admin_id"];
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
                int id = (int)Session["admin_id"];
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