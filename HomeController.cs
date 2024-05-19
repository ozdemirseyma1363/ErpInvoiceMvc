using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using Antlr.Runtime.Misc;
using System.Xml.Linq;
using FaturaWeb.Models;
using Newtonsoft.Json.Linq;
using System.Data.Entity.Migrations.Sql;
namespace FaturaWeb.Controllers
{
    public class HomeController : Controller
    {
        //public ActionResult partial()
        //{
        //    return PartialView("_deneme");
        //    //PartialView("_deneme");
        //}
        public ActionResult Index(Models.PostObject fm)
        {
            DataTemp.TempFaturaDetay = new List<FaturaDetay>();
            return View(fm);
        }
        public ActionResult Stoklar(string id)
        {
            DataModel dm = new DataModel();
            int No = 0;
            int.TryParse(id, out No);

            Stok u = new Stok();

            if (No > 0)
            {
                u = dm.Stoks.Where(q => q.No == No).FirstOrDefault();
            }
            return View(u);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StokEkle(Stok stok)
        {
            Stok s = new Stok();
            s.Ad = stok.Ad;
            s.Barkod = stok.Barkod;
            s.Kod = stok.Kod;
            s.BirimFiyat = stok.BirimFiyat;
            DataModel dm = new DataModel();
            dm.Stoks.Add(s);
            dm.SaveChanges();
            return View("Stoklar");//dm.stoks            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StokSil(int id)
        {
            DataModel dm = new DataModel();
            var model = dm.Stoks.Where(x => x.No == id).FirstOrDefault();
            var model2 = dm.FaturaDetays.Where(x => x.StokNo == id).FirstOrDefault();
            if (model2 != null)
            {
       dm.FaturaDetays.Remove(model2);

            }
            dm.Stoks.Remove(model);
            dm.SaveChanges();
            return View("Stoklar");
        }
        [HttpPost]
        public ActionResult StokDüzenle(Stok stok)
        {
            DataModel dm = new DataModel();
            return View();
        }
        public ActionResult Edit(string id)
        {
     
            DataModel dm = new DataModel();
            int No = 0;
            int.TryParse(id, out No);

            Fatura u = new Fatura();

            if (No > 0)
            {
                u = dm.Faturas.Where(q => q.No == No).FirstOrDefault();
            }
            return View(u);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Save(Fatura po)
        {
            Models.ResponseObject ro = new Models.ResponseObject();
         
            ro.Result = false;
            ro.Message = "";
            DataModel dm = new DataModel();
            Fatura u = new Fatura();
            if (po.No > 0)
            {
                u = dm.Faturas.Where(q => q.No == po.No).FirstOrDefault();
            }
            u.FaturaNumarasi = po.FaturaNumarasi;
            u.CariNo = po.CariNo;

            u.Tarih= DateTime.Now;
            if (u.No == 0)
            {
                u.Toplam =DataTemp.TempFaturaDetay.Sum(sm => sm.Tutar + sm.KdvTutar);
                u.Kdv= DataTemp.TempFaturaDetay.Sum(sm => sm.KdvTutar);
            }
            else
            {
                var tr = dm.FaturaDetays.Where(q => q.FaturaNo == u.No).ToList();
                u.Toplam=tr.Sum(sm => sm.Tutar);
                u.Kdv=tr.Sum(sm => sm.Tutar + sm.KdvTutar);
            }
            if (po.No == 0)
            {
                dm.Faturas.Add(u);
                foreach (var item in DataTemp.TempFaturaDetay.Where(q => q.No < 0).ToList())//
                {
                    FaturaDetay dt = new FaturaDetay();
                    dt.FaturaNo = item.FaturaNo;
                    dt.Miktar = item.Miktar;
                   dt.KdvOrani = 18;
                    dt.StokNo = item.StokNo;
                    dt.FaturaNo = item.FaturaNo;
                    var k = dm.Stoks.Where(s => s.No == dt.StokNo).FirstOrDefault();
                    dt.BirimFiyat = k.BirimFiyat;
                    dt.Tutar = dt.BirimFiyat * dt.Miktar;
                    dt.KdvTutar = dt.Tutar * (dt.KdvOrani) / 100;
                    dm.FaturaDetays.Add(dt);
                }
                if (po.No != 0)
                {
                    foreach (var item in dm.FaturaDetays.Where(q => q.FaturaNo== po.No).ToList())
                    {
                        FaturaDetay dt = new FaturaDetay();
                        dt.FaturaNo = item.FaturaNo;
                        dt.Miktar = item.Miktar;
                        dt.KdvOrani = 18;
                        dt.StokNo = item.StokNo;
                        dt.FaturaNo = item.FaturaNo;
                        var k = dm.Stoks.Where(s => s.No == dt.StokNo).FirstOrDefault();
                        dt.BirimFiyat = k.BirimFiyat;
                        dt.Tutar = dt.BirimFiyat * dt.Miktar;
                        dt.KdvTutar = dt.Tutar * (dt.KdvOrani) / 100;
                    }
                }
             
            }
            try
            {
                dm.SaveChanges();
                ro.Result = true;
                ro.Message = "Saved.";
            }
            catch (Exception ex)
            {
                ro.Message = ex.Message;
            }
            return Json(ro);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteFtr(Fatura fa)
        {
            Models.ResponseObject ro = new Models.ResponseObject();
            ro.Result = false;
            ro.Message = "";
            DataModel dm = new DataModel();

            if (fa.No > 0)
            {
                var k = dm.FaturaDetays.Where(q => q.FaturaNo == fa.No).ToList();
                foreach(var item in k)
                {
                    dm.FaturaDetays.Remove(item);
                }
                var u = dm.Faturas.Where(q => q.No == fa.No).FirstOrDefault();

                dm.Faturas.Remove(u);
            }
            else
            {
                ro.Message = "Null Value Posted!";
                return Json(ro);
            }
            try
            {
                dm.SaveChanges();
                ro.Result = true;
                ro.Message = "Deleted.";
            }
            catch (Exception ex)
            {
                ro.Message = ex.Message;

            }
            return Json(ro,JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteDty(FaturaDetay ftd)
        {
            Models.ResponseObject ro = new Models.ResponseObject();
            ro.Result = false;
            ro.Message = "";
            DataModel dm = new DataModel();

            if (ftd.No > 0)
            {
                var u = dm.FaturaDetays.Where(q => q.No == ftd.No).FirstOrDefault();
                var detay=dm.Faturas.Where(q => q.No == ftd.FaturaNo).FirstOrDefault();
                detay.Toplam = detay.Toplam - (u.Tutar) - u.KdvTutar;
                dm.FaturaDetays.Remove(u);
              
            }
            if (ftd.No < 0)
            {
                var tmp = DataTemp.TempFaturaDetay.Where(q => q.No == ftd.No).FirstOrDefault();
                
                if (tmp !=null)
                {

                    DataTemp.TempFaturaDetay.Remove(tmp);


                }
            }
            

            //else
            //{
            //    ro.Message = "Null Value Posted!";
            //    return Json(ro);
            //}
            try
            {
                dm.SaveChanges();
                ro.Result = true;
                ro.Message = "Deleted.";
            }
            catch (Exception ex)
            {
                ro.Message = ex.Message;

            }
            return Json(ro, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveFtr(FaturaDetay fd)
        {
            if ((fd.FaturaNo == 0)&&DataTemp.TempFaturaDetay.Count==0)//
            {
                DataTemp.TempFaturaDetay = new List<FaturaDetay>();
            }

            Models.ResponseObject ro = new Models.ResponseObject();
            ro.Result = false;
            ro.Message = "";
            DataModel dm = new DataModel();
            if (fd.FaturaNo != 0)
            {
                if (fd.No == 0)
                {
                    FaturaDetay k= new FaturaDetay();//yeni faturaya ürün ekleme
                    k.Miktar = fd.Miktar;
                    k.KdvOrani = 18;
                    k.StokNo = fd.StokNo;
                    k.FaturaNo = fd.FaturaNo;
                    var t = dm.Stoks.Where(s => s.No == k.StokNo).FirstOrDefault();
                    k.BirimFiyat = t.BirimFiyat;
                    k.Tutar = k.BirimFiyat * k.Miktar;
                    k.KdvTutar = k.Tutar * (k.KdvOrani) / 100;
                    dm.FaturaDetays.Add(k);
                    var ftr = dm.Faturas.Where(s => s.No == k.FaturaNo).FirstOrDefault();
                    if (ftr != null)
                    {
                        ftr.Toplam = ftr.Toplam + (k.Tutar + k.KdvTutar);
                    }
                }
                else
                {

                    var u = dm.FaturaDetays.Where(q => q.No == fd.No).FirstOrDefault();
                    u.Miktar = fd.Miktar;
                    u.KdvOrani = 18;
                    u.StokNo = fd.StokNo;
                    u.FaturaNo = fd.FaturaNo;
                    var k = dm.Stoks.Where(s => s.No == u.StokNo).FirstOrDefault();
                    u.BirimFiyat =k.BirimFiyat;
                    u.Tutar = k.BirimFiyat * u.Miktar;
                    u.KdvTutar = u.Tutar * (u.KdvOrani) / 100;
                    var ftr = dm.Faturas.Where(s => s.No == u.FaturaNo).FirstOrDefault();
                    if (ftr != null)
                    {
                        ftr.Toplam = u.Fatura.FaturaDetays.Sum(sm => sm.Tutar + sm.KdvTutar);
                        ftr.Kdv= u.Fatura.FaturaDetays.Sum(sm => sm.KdvTutar);
                    }


                }

            }
            if (fd.FaturaNo == 0)
            {
                if (fd.No == 0)
                {
                    FaturaDetay ftrd = new FaturaDetay();
                    ftrd.No = -(DataTemp.TempFaturaDetay.Count + 1);
                    ftrd.Miktar = fd.Miktar;
                    ftrd.FaturaNo = fd.FaturaNo;
                    ftrd.KdvOrani = 18;
                    ftrd.StokNo = fd.StokNo;
                    var k = dm.Stoks.Where(s => s.No == ftrd.StokNo).FirstOrDefault();
                    ftrd.BirimFiyat = k.BirimFiyat;
                    ftrd.Tutar = ftrd.BirimFiyat * ftrd.Miktar;
                    ftrd.KdvTutar = ftrd.Tutar * (ftrd.KdvOrani) / 100;
                    DataTemp.TempFaturaDetay.Add(ftrd);
                }
                if (fd.No < 0)
                {

                    var u = DataTemp.TempFaturaDetay.Where(q => q.No == fd.No).FirstOrDefault();
                    u.Miktar = fd.Miktar;
                    u.KdvOrani = 18;
                    u.StokNo = fd.StokNo;
                    u.FaturaNo = fd.FaturaNo;
                    var k = dm.Stoks.Where(s => s.No == u.StokNo).FirstOrDefault();
                    u.BirimFiyat = k.BirimFiyat;
                    u.Tutar = k.BirimFiyat * u.Miktar;
                    u.KdvTutar = u.Tutar * (u.KdvOrani) / 100;
                    //var ftr = dm.Faturas.Where(s => s.No == u.FaturaNo).FirstOrDefault();
                    //if (ftr != null)
                    //{
                    //    ftr.Toplam = u.Sum(sm => sm.Tutar + sm.KdvTutar);
                    //    ftr.Kdv = u.Fatura.FaturaDetays.Sum(sm => sm.KdvTutar);
                    //}


                }
            }
            //else {


            //}

            try
                {
                    dm.SaveChanges();

                    ro.Result = true;
                    ro.Message = "Saved.";
                }
                catch (Exception ex)
                {
                    ro.Message = ex.Message;

                }
            return Json(ro);
            }
        }
    }


















































//dm.Faturas.Add(f);
//f.FaturaNumarasi = (txtFaturaNumara.Text);
//f.Tarih = DateTime.Now;
//f.CariNo = cariNo;
//f.Kdv = DataTemp.TempFaturaDetay.Sum(sm => sm.KdvTutar);
//f.Toplam = DataTemp.TempFaturaDetay.Sum(sm => sm.Tutar + sm.KdvTutar);
// dm.Faturas.Add(f);


//FaturaDetay ftd = new FaturaDetay();

//            int selectedId = 1;
//            ViewBag.ItemsSelect = new SelectList(dm.Stoks, "No", "Ad", selectedId);
//            < !--@Html.DropDownListFor(m => ftr.StokNo, (SelectList)ViewBag.ItemsSelect)-- >
//                                            @*@Html.DropDownListFor(m => ftr.StokNo, new SelectList(dm.Stoks, "No", "Ad", 1), new { @class = "form-control" }) *@
//                                            < !--< script >
//$("document").ready(function() {
//    $('#ItemId').val('@ftr.StokNo');
//            });</ script > -->


//Fatura fa = new Fatura();
//fa.FaturaNumarasi = (fd.Fatura.FaturaNumarasi);
//fa.Tarih = DateTime.Now;
//fa.CariNo = fd.Fatura.CariNo;
//fa.Kdv = fd.KdvTutar;
//dm.Faturas.Add(fa);
//foreach (var item in fd.Fatura.FaturaDetays.ToList())
//{
//    FaturaDetay fd1 = new FaturaDetay();
//    fd1.Fatura = f;
//    fd1.Tutar = item.Tutar;
//    fd1.BirimFiyat = item.BirimFiyat;
//    fd1.KdvTutar = item.KdvTutar;
//    fd1.KdvOrani = item.KdvOrani;
//    fd1.Miktar = item.Miktar;
//    fd1.StokNo = item.StokNo;
//    dm.FaturaDetays.Add(fd);
//    dm.SaveChanges();



//FaturaDetay fd = new FaturaDetay();
//fd.No = -(DataTemp.TempFaturaDetay.Count + 1);
//fd.KdvOrani = Convert.ToDecimal(txtKdvOrani.Text);
//fd.BirimFiyat = Convert.ToDecimal(txtBirimFiyat.Text);
//fd.Miktar = Convert.ToDecimal(txtMiktar.Text);
//fd.StokNo = stokNo;
//fd.Tutar = fd.BirimFiyat * fd.Miktar;
//fd.KdvTutar = fd.Tutar * (fd.KdvOrani) / 100;
//fd.Stok = dm.Stoks.Where(q => q.No == stokNo).FirstOrDefault();
//DataTemp.TempFaturaDetay.Add(fd);

//if (fd.No == 0 && u.No == 0)
//   {
//    var ftrd = dm.Faturas.Where(q => q.No == fd.FaturaNo).FirstOrDefault();
//    ftrd.FaturaNumarasi = fd.Fatura.FaturaNumarasi;
//    ftrd.FaturaDetays = f.FaturaDetays.ToList();
//}

//if (fd.No > 0)
//{
//    u = dm.FaturaDetays.Where(q => q.No == fd.No).FirstOrDefault();
//}
//else
//{
//    u = DataTemp.TempFaturaDetay.Where(q => q.No == fd.No).FirstOrDefault();
//}


//                                    @*< tr >

//                                                < td >

//                                                    < select name = "cars" id = "cars" >
//                                                        @foreach(var item in lst)
//                                                        {
//                                                            < option value = "@item.No" > @item.Ad </ option >
//}
//                                                    </ select >



//                                                </ td >

//                                            </ tr > *@

//    //if(fd.No == 0)
//{
//    u=FAturaDetayList.TempFaturaDetay.Where(q => q.No == fd.No).FirstOrDefault();
//}
//public ActionResult Delete(string id)
//{
//    DataModel dm = new DataModel();

//    int No = 0;
//    int.TryParse(id, out No);

//    Fatura u = new Fatura();

//    if (No > 0)
//    {
//        u = dm.Faturas.Where(q => q.No == No).FirstOrDefault();

//    }
//    return View(u);
//}