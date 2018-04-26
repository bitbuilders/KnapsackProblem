using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Knapsack
{
    public enum SpecifiedParameter
    {
        WEIGHT,
        VALUE
    }

    class KnapsackItem
    {
        public KnapsackItem(string name = "GenericItem", int weight = 0, int value = 0)
        {
            this.name = name;
            this.weight = weight;
            this.value = value;
        }

        public string name;
        public int weight;
        public int value;

        public static int Total(IEnumerable<KnapsackItem> list, SpecifiedParameter parameter)
        {
            int sum = 0;

            foreach (KnapsackItem item in list)
            {
                switch (parameter)
                {
                    case SpecifiedParameter.VALUE:
                        sum += item.value;
                        break;
                    case SpecifiedParameter.WEIGHT:
                        sum += item.weight;
                        break;
                }
            }

            return sum;
        }

        public static List<KnapsackItem> SortBy(IEnumerable<KnapsackItem> list, SpecifiedParameter parameter)
        {
            List<KnapsackItem> newList = null;
            switch (parameter)
            {
                case SpecifiedParameter.VALUE:
                    newList = list.OrderBy(x => x.value).ToList();
                    break;
                case SpecifiedParameter.WEIGHT:
                    newList = list.OrderBy(x => x.weight).ToList();
                    break;
            }

            return newList;
        }
    }

    class Program
    {
        static List<KnapsackItem> knapsackItems;
        static List<KnapsackItem> chosenItems;
        static List<KnapsackItem> seenItems;
        static int maxCapacity;

        static void Main(string[] args)
        {
            RestartProgram();
        }

        static void RestartProgram()
        {
            knapsackItems = new List<KnapsackItem>();
            chosenItems = new List<KnapsackItem>();
            seenItems = new List<KnapsackItem>();
            ParseItemsFromFile(GetFilePathFromUser());
            PrintItems(knapsackItems);
            PrintItems(KnapsackItem.SortBy(knapsackItems, SpecifiedParameter.VALUE));
            FindBestItems();
        }

        static void FindBestItems()
        {
            for (int i = 0; i < knapsackItems.Count; ++i)
            {
                KnapsackItem item = knapsackItems[i];
                if (KnapsackItem.Total(chosenItems, SpecifiedParameter.WEIGHT) + item.weight <= maxCapacity)
                {
                    chosenItems.Add(item);
                }
                else
                {
                    seenItems.Add(item);
                    for (int j = seenItems.Count - 1; j >= 0; --j)
                    {

                    }

                }
            }
        }

        static void ShoveItemIntoSack(KnapsackItem itemToFit)
        {
            int currentWeight = KnapsackItem.Total(chosenItems, SpecifiedParameter.WEIGHT);
            int currentValue = KnapsackItem.Total(chosenItems, SpecifiedParameter.VALUE);
            int mostValue = currentValue;
            int mostWeight = currentWeight;
            List<KnapsackItem> itemsPossiblyRemoved = new List<KnapsackItem>();
            for (int i = 0; i < seenItems.Count; ++i)
            {
                KnapsackItem currentItem = chosenItems[i];
                if (mostWeight - currentItem.weight + itemToFit.weight <= maxCapacity &&
                    mostValue - currentItem.value + itemToFit.value > mostValue)
                {
                    mostValue = currentValue - currentItem.value + itemToFit.value;
                    mostWeight = mostWeight - currentItem.weight + itemToFit.weight;
                }
            }
        }

        static string GetFilePathFromUser()
        {
            Console.WriteLine("Please enter in a file path with a list of items:");

            string path = "";
            bool validInput = false;
            do
            {
                path = Console.ReadLine();

                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("Woops, try entering in something!\n");
                }
                else if (!File.Exists(path))
                {
                    Console.WriteLine("That's not a valid file path, please try again:");
                }
                else
                {
                    validInput = true;
                }
            } while (!validInput);

            return path;
        }

        static void ParseItemsFromFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);

            int.TryParse(lines[0], out maxCapacity);
            int numOfItems = 0;
            int.TryParse(lines[1], out numOfItems);

            for (int i = 2; i < numOfItems; ++i)
            {
                string[] itemContents = lines[i].Split(',');

                KnapsackItem item = new KnapsackItem();
                item.name = itemContents[0];
                int.TryParse(itemContents[1], out item.weight);
                int.TryParse(itemContents[2], out item.value);

                knapsackItems.Add(item);
            }
        }

        static void PrintItems(List<KnapsackItem> items)
        {
            StringBuilder sb = new StringBuilder("\n");
            foreach (KnapsackItem item in items)
            {
                sb.Append(item.name);
                sb.Append(" - ");
                sb.Append(item.weight);
                sb.Append(" lbs. at $");
                sb.Append(item.value);
                sb.Append("\n");
            }

            Console.WriteLine(sb.ToString());
        }
    }
}
