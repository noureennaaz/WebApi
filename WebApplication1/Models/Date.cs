using Microsoft.AspNetCore.Components.Forms;
using MongoDB.Bson;
namespace WebApplication1.Models
{
    public class Date
    {
        public string DateType { get; set; }
        public DateTime DateOnly { get; set; }

    //     public Date(){
    //     DateType="dd-mm-yy";
    //     DateOnly=DateTime.Now;
    // }

    }
    
    
}