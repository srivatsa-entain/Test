using System;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Racing.Models;
using SQLitePCL;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Racing.Services
{
    public class RacingService : Racing.RacingBase
    {
        private readonly ILogger<RacingService> _logger;
        public RacingService(ILogger<RacingService> logger)
        {
            _logger = logger;
        }

        //GetRaceById
        public override Task<GetRacebyIdResponse> GetRaceById(GetRacebyIdRequest request, ServerCallContext context)
        {
            using var db = new RacingContext();
            var rsp = new GetRacebyIdResponse();
            try
            {
                var races = db.Races.Where(r => r.Id == request.Filter.Id);

                if (races != null)
                {
                    var race = races.FirstOrDefault();

                    DateTime.TryParse(race?.AdvertisedStartTime, out var dateTime);
                    if (race != null)
                    {
                        rsp.Race = new Race
                        {
                            Name = race?.Name,
                            Id = race?.Id != null ? (long)race.Id : 0,
                            MeetingId = race?.MeetingId != null ? (long)race.MeetingId : 0,
                            Number = race?.Number != null ? (long)race.Number : 0,
                            Visible = race?.Visible == 1,
                            AdvertisedStartTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(dateTime.ToUniversalTime()),
                            Status = DateTime.Compare(Convert.ToDateTime(race.AdvertisedStartTime), DateTime.Now.ToUniversalTime()) < 0 ? "Open" : "Close"
                        };
                    }
                }
                return Task.FromResult(rsp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
        }

        public override Task<ListRacesResponse> ListRaces(ListRacesRequest request, ServerCallContext context)
        {
            using var db = new RacingContext();

            //Get the explicit declaration so can easily filter
            //As we need to limit the races to a maximum of 20, we need to get the correct count for Visible races
            //Ideal solution would be to update the DB Proc to accept the parameter and return dataset accordingly
            try
            {
                var races = db.Races.ToList();

                if (request.Filter.Visible)
                {
                    races = (List<Models.Race>)races.Where(r => r.Visible == 1).Take(20).OrderBy(r => r.AdvertisedStartTime).ToList();
                }
                else
                {
                    races = (List<Models.Race>)races.Take(20).OrderBy(r => r.AdvertisedStartTime).ToList();
                }

                var rsp = new ListRacesResponse();

                // loop over races, assign to response
                foreach (var race in races)
                {
                    // TODO: add some error handling to ensure the date is valid
                    DateTime.TryParse(race?.AdvertisedStartTime, out var dateTime);
                    rsp.Races.Add(
                        new Race
                        {
                            Name = race?.Name,
                            Id = race?.Id != null ? (long)race.Id : 0,
                            MeetingId = race?.MeetingId != null ? (long)race.MeetingId : 0,
                            Number = race?.Number != null ? (long)race.Number : 0,
                            Visible = race?.Visible == 1,
                            AdvertisedStartTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(dateTime.ToUniversalTime()),
                            Status = DateTime.Compare(Convert.ToDateTime(race?.AdvertisedStartTime), DateTime.Now.ToUniversalTime()) < 0 ? "Open" : "Close"
                        }
                    );
                }
                return Task.FromResult(rsp);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
        } 
    }
}
