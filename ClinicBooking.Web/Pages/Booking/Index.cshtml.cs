using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicBooking.Web.Pages.Booking
{
    public class IndexModel : PageModel
    {
        public int SelectedDoctorId { get; set; }

        public void OnGet(int? doctorId)
        {
            SelectedDoctorId = doctorId ?? 1;
        }
    }
}
