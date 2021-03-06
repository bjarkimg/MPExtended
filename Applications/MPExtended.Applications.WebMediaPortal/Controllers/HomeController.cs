﻿#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        //
        // GET: /Home/
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NewMovies()
        {
            try
            {
                var tmp = MPEServices.MAS.GetMoviesDetailedByRange(Settings.ActiveSettings.MovieProvider, 0, 3, sort: SortBy.DateAdded, order: OrderBy.Desc);
                return PartialView(tmp);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ActionResult NewEpisodes()
        {
            try
            {
                var tmp = MPEServices.MAS.GetTVEpisodesDetailedByRange(Settings.ActiveSettings.TVShowProvider, 0, 3, SortBy.TVDateAired, OrderBy.Desc);
                var list = tmp.Select(x => new EpisodeModel
                {
                    Episode = x,
                    Season = MPEServices.MAS.GetTVSeasonDetailedById(x.PID, x.SeasonId),
                    Show = MPEServices.MAS.GetTVShowDetailedById(x.PID, x.ShowId)
                }).ToList();

                return PartialView(list);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ActionResult NewRecordings()
        {
            try
            {
                var tmp = MPEServices.TAS.GetRecordingsByRange(0, 4, SortField.StartTime, SortOrder.Desc);
                return PartialView(tmp);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ActionResult CurrentSchedules()
        {
            try
            {
                var schedules = MPEServices.TAS.GetSchedules()
                    .Where(x => x.StartTime.Date == DateTime.Now.Date)
                    .Select(x => new ScheduleViewModel(x))
                    .ToList();
                return PartialView(schedules);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
