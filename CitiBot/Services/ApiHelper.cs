using CitiBot.CognitiveModels;
using CitiBot.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CitiBot.Services
{
    public class ApiHelper: IApiHelper
    {
        private readonly HttpClient _httpclient;
        private readonly IConfiguration configuration;

        public ApiHelper(IConfiguration configuration)
        {
            _httpclient = new HttpClient();
            this.configuration = configuration;
            _httpclient.DefaultRequestHeaders.Add("Authorization", "Bearer " + configuration["Bearer"]);
        }

        public async Task<T> Book_Appointment<T>(AppointmentDetails appointmentDetails)
        {
            string mystr = "{\"scheduleAppointmentRequest\":{\"Start\":\""+ appointmentDetails.date+ " "+ appointmentDetails.time+ "\",\"End\":\""+appointmentDetails.date+" "+appointmentDetails.time+"\",\"Notes\":\"test\",\"PhysicianId\":\"1\",\"FacilityId\":\"2\",\"PatientId\":\"3\",\"AppointmentType\":\"test\"}}";
            var data = new StringContent(mystr, Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpclient.PostAsync(configuration["Book_Appointment"], data);
                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
            }
            return default(T);
        }

        public async Task<T> Get_Appointment_Slots<T>(string date)
        {
            string mystr = "{\"request\": {\"StartDate\": \"" + date + " 00:00:00\",\"EndDate\": \"" + date + " 00:00:00\",\"facilityIds\":[\"2\"],\"physicianIds\":[\"1\"],\"AppointmentTypes\":[\"test\"]}}";
            var data = new StringContent(mystr, Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpclient.PostAsync(configuration["Get_Appointment_Slots"], data);
                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
            }
            return default(T);
        }

        public async Task<T> Get_Dates<T>()
        {
            var data = new StringContent("{\"request\":{\"facilityIds\":[\"2\"],\"physicianIds\":[\"1\"]}}", Encoding.UTF8, "application/json");
            try
            {
               var response = await _httpclient.PostAsync(configuration["Get_Dates"], data);
               return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {  
            }
            return default(T);
        }
        public async Task<T> Get_Previous_Appointment<T>()
        {
            string mystr = "{\"PatientId\":\"12345\"}";
            var data = new StringContent(mystr, Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpclient.PostAsync(configuration["Get_Previous_Appointment"], data);
                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
            }
            return default(T);
        }
    }
}
