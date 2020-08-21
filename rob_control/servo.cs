using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rob_control
{
    public class servo
    {
        public string attachPart;
        public int value;
        public void Switch(servo first,servo second)
        {
            servo temp = first.MemberwiseClone() as servo;
            first.attachPart = second.attachPart;
            first.value = second.value;
            second.attachPart = temp.attachPart;
            second.value = temp.value;
        }

        //servo(string PartName,)
        
    }
}
