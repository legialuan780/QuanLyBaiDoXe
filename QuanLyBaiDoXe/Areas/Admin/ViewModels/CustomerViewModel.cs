using QuanLyBaiDoXe.Models.Entities;

namespace QuanLyBaiDoXe.Areas.Admin.ViewModels
{
    public class CustomerViewModel
    {
        public List<KhachHang> Customers { get; set; } = new();
        public string? SearchKeyword { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int TotalCustomers { get; set; }
        public int CustomersWithAccount { get; set; }
        public int CustomersWithMonthlyTicket { get; set; }
    }
}
