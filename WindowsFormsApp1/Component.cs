using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace WindowsFormsApp1
{
    public class Component
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int IsRoot { get; set; }
        public int Quantity { get; set; }
        public List<Component> Children { get; set; } = new List<Component>(); // Рекурсивная ссылка
        public string JsonChildren { get; set; }

        public Component(int id, string name, string jsonChildren)
        {
            Id = id;
            Name = name;
            JsonChildren = jsonChildren;

            Children = JsonSerializer.Deserialize<List<Component>>(jsonChildren);
        }

        public void Init()
        {
            List<MyData> myData = null;
            try
            {
                myData = JsonSerializer.Deserialize<List<MyData>>(JsonChildren);
            }
            catch { }

            foreach(MyData md in myData)
            {
                Children.Add(new Component { Id = md.id, Quantity = md.count });
            }

        }

        public Component(int id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }

        /*public Component(int id, string name, int isRoot, int quantity)
        {
            Id = id;
            Name = name;
            IsRoot = isRoot;
            Quantity = quantity;
        }*/

        public Component()
        {
        }

    }
}
