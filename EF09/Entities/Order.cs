using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF09.Entities;
[Table("Orders", Schema = "Sales")]

internal class Order
{

    [Key]
    public int Id { get; set; }
    public string CustomerEmail { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

}
