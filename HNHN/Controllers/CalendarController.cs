using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HNHB.Models;
using System.Globalization;
using System.Data.Entity.Validation;
using System.Diagnostics;
using WebMatrix.Data;
using HNHB.Models.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Data.Entity;
using Newtonsoft.Json;
using HNHB.HelperExtension;
using System.Data;
using HNHB.DAO;



namespace HNHB.Controllers
{
   
        public class CalendarController : Controller
        {
            //Get database
            Entities ent = new Entities();          
            //Get contructor CalendarDAO
            CalendarDAO CalendarDAO = new CalendarDAO();
            [Authorize(Roles = "Administrator")]
            public ActionResult Index()
            {
                return View();
            }

            public ActionResult PatialCalendar()
            {
                return View();
            }
            //Update Event
            [Authorize(Roles = "Administrator")]
            public void UpdateEvent(int id, string NewEventStart, string NewEventEnd)
            {
                CalendarDAO.UpdateEvents(id, NewEventStart, NewEventEnd);
                
            }
            //Delete Event
            [Authorize(Roles = "Administrator")]
            public void DeleteEvent(int id)
            {
                CalendarDAO.DeleteDiaryEvent(id);
            }

            [Authorize(Roles = "Administrator")]
            public bool SaveEvent(string Title, string NewEventDate, string NewEventAddress,
                string NewEventDescription, string NewEventSTime,
                string NewEventETime, string NewEventDuration, string NewEventSymbol)
            {
                NewEventDuration = "1";
                int symbol = Int32.Parse(NewEventSymbol);
                return CreateNewEvent(Title, NewEventDate, NewEventAddress, NewEventDescription,
                    NewEventSTime, NewEventETime, NewEventDuration, symbol);
            }
            // Get data of Event
            public JsonResult GetEvents(DateTime start, DateTime end)
            {
                var ApptListForDate = CalendarDAO.LoadAllEventsInDateRange(start, end);

                var eventList = from e in ApptListForDate
                                select new
                                {
                                    id = e.ID,
                                    title = e.Title,
                                    start = e.StartDateString,
                                    estart = e.StartDateString,
                                    end = e.EndDateString,
                                    color = e.StatusColor,
                                    className = e.ClassName,
                                    someKey = e.UserId,
                                    allDay = true,
                                    address = e.Address,
                                    description = e.Descriptions
                                };
                var rows = eventList.ToArray();
                return Json(rows, JsonRequestBehavior.AllowGet);
            }

            //public static List<CalendarEvents> LoadAllEventsInDateRange(DateTime start, DateTime end)
            //{
            //    var fromDate = start;
            //    var toDate = end;

            //    using (Entities ent = new Entities())
            //    {
            //        var rslt = ent.EventCalendars.Where(s => s.DateTimeScheduled >= fromDate
            //            && s.EndDay <= toDate
            //            && s.IsActive == true);

            //        List<CalendarEvents> result = new List<CalendarEvents>();
            //        foreach (var item in rslt)
            //        {
            //            CalendarEvents rec = new CalendarEvents();
            //            rec.ID = item.ID;
            //            rec.UserId = item.UserID;
            //            string StringDate = string.Format("{0:yyyy-MM-dd}", item.DateTimeScheduled);
            //            rec.StartDateString = item.DateTimeScheduled.ToString("yyyy-MM-dd");
            //            rec.EndDateString = item.EndDay.ToString("yyyy-MM-dd");
            //            int enColor = EventExpired(rec.ID, rec.EndDateString, item.StatusENUM);
            //            rec.Title = item.Title;
            //            rec.StatusString = Enums.GetName<EventStatus>((EventStatus)enColor);
            //            rec.StatusColor = Enums.GetEnumDescription<EventStatus>(rec.StatusString);
            //            string ColorCode = rec.StatusColor.Substring(0, rec.StatusColor.IndexOf(":"));
            //            rec.ClassName = rec.StatusColor.Substring(rec.StatusColor.IndexOf(":") + 1, rec.StatusColor.Length - ColorCode.Length - 1);
            //            rec.StatusColor = ColorCode;
            //            rec.Address = item.Address;
            //            rec.Descriptions = item.Description;
            //            result.Add(rec);
            //        }

            //        return result;
            //    }
            //}

            //private static DateTime ConvertFromUnixTimestamp(double timestamp)
            //{
            //    var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            //    return origin.AddSeconds(timestamp);
            //}

            public bool CreateNewEvent(string Title, string NewEventDate, string NewEventAddress,
                string NewEventDescription, string NewEventSTime,
                string NewEventETime, string NewEventDuration, int EventSymbol)
            {
                try
                {
                    Entities ent = new Entities();
                    EventCalendar evt = new EventCalendar();
                    evt.UserID = User.Identity.GetUserId<int>();
                    evt.Title = Title;
                    evt.Address = NewEventAddress;
                    evt.DateTimeScheduled = DateTime.Parse(NewEventDate);
                    evt.Description = NewEventDescription;
                    evt.EndDay = evt.DateTimeScheduled;
                    evt.StatusENUM = EventSymbol;
                    evt.IsActive = true;
                    ent.EventCalendars.Add(evt);
                    ent.SaveChanges();
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName,
                                validationError.ErrorMessage);
                        }
                    }

                    return false;
                }
                return true;
            }

            //public static void UpdateEvents(int id, string NewEventStart, string NewEventEnd)
            //{
            //    using ( Entities ent = new Entities())
            //    {
            //        var rec = ent.EventCalendars.FirstOrDefault(s => s.ID == id);
            //        if (rec != null)
            //        {
            //            DateTime DateTimeStart = DateTime.Parse(NewEventStart, null, DateTimeStyles.RoundtripKind).ToLocalTime(); // and convert offset to localtime
            //            rec.DateTimeScheduled = DateTimeStart;
            //            if (!String.IsNullOrEmpty(NewEventEnd))
            //            {
            //                TimeSpan span = DateTime.Parse(NewEventEnd, null, DateTimeStyles.RoundtripKind).ToLocalTime() - DateTimeStart;
            //                rec.EndDay = DateTime.Parse(NewEventEnd, null, DateTimeStyles.RoundtripKind).ToLocalTime();
            //            }
            //            ent.SaveChanges();
            //        }
            //    }
            //}

            //public static void DeleteDiaryEvent(int id)
            //{
            //    using (Entities ent = new Entities())
            //    {
            //        var rec = ent.EventCalendars.FirstOrDefault(s => s.ID == id);
            //        if (rec != null)
            //        {
            //            rec.IsActive = false;
            //            ent.SaveChanges();
            //        }
            //    }

            //}

            //public static int EventExpired(int id, string endDay, int enumColor)
            //{
            //    int color = enumColor;
            //    DateTime eEndday = DateTime.Parse(endDay);
            //    if (eEndday < DateTime.Now.Date)
            //    {
            //        color = 2;
            //    }
            //    else if (enumColor == 1)
            //    {
            //        color = 1;
            //    }
            //    else
            //    {
            //        color = 0;
            //    }
            //    return color;
            //}



        }
    }
