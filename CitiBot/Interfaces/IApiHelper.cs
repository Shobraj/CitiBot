using CitiBot.CognitiveModels;
using System.Threading.Tasks;

namespace CitiBot.Interfaces
{
    public interface IApiHelper
    {
        Task<T> Get_Previous_Appointment<T>();
        Task<T> Get_Dates<T>();
        Task<T> Get_Appointment_Slots<T>(string date);
        Task<T> Book_Appointment<T>(AppointmentDetails appointmentDetails);

    }
}
