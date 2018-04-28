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
            Console.WriteLine($"\nMaximum Bag Capacity: {maxCapacity}\n\nStarting Item Pool:");
            PrintItems(knapsackItems);
            FindBestItems();
            Console.WriteLine("Items I chose to put into knapsack:");
            PrintItems(chosenItems);
            Console.WriteLine($"Total Weight: {KnapsackItem.Total(chosenItems, SpecifiedParameter.WEIGHT)} lbs.");
            Console.WriteLine($"Total Value: ${KnapsackItem.Total(chosenItems, SpecifiedParameter.VALUE)}\n");
        }

        static void FindBestItems()
        {
            List<KnapsackItem> weightedItems = KnapsackItem.SortBy(knapsackItems, SpecifiedParameter.WEIGHT);
            int currentWeight = KnapsackItem.Total(chosenItems, SpecifiedParameter.WEIGHT);

            for (int i = 0; i < weightedItems.Count; ++i)
            {
                KnapsackItem item = weightedItems[i];
                seenItems.Add(item);
                if (currentWeight + item.weight <= maxCapacity)
                {
                    chosenItems.Add(item);
                    currentWeight += item.weight;
                }
                else
                {
                    break;
                }
            }

            // Loop through lightest least valuable and keep removing them until you have enough room for heavy item
            // If at that point the addition of a heavy item would have an overall NET LOSS, don't do it, move on
            // If it is worth it to add the heavy item, then you will need to refresh the list and sort it again
            // Removed items should be added to a list that will be checked after the heavy items are checked
            // This way it will re-check the ones that were removed and there may be a large enough weight gap to slip it in
            // This process will repeat until nothing can be done
            // REMEMBER: Check and see if an item can be put in WITHOUT removing anything

            int chosenCount = chosenItems.Count;
            List<KnapsackItem> lightestByValue = KnapsackItem.SortBy(chosenItems, SpecifiedParameter.VALUE);
            List<KnapsackItem> removedItems = new List<KnapsackItem>();
            int startingValue = KnapsackItem.Total(lightestByValue, SpecifiedParameter.VALUE);
            int startingWeight = KnapsackItem.Total(chosenItems, SpecifiedParameter.WEIGHT);
            int weightLoss = 0;
            int valueLoss = 0;
            int lightIndex = 0;
            for (int i = weightedItems.Count - 1; i >= chosenCount; --i)
            {
                KnapsackItem heaviestItem = weightedItems[i];
                for (int j = lightIndex; j < lightestByValue.Count; ++j)
                {
                    KnapsackItem lightestItem = lightestByValue[j];
                    removedItems.Add(lightestItem);
                    weightLoss += lightestItem.weight;
                    valueLoss += lightestItem.value;
                    if (startingWeight - weightLoss + heaviestItem.weight <= maxCapacity)
                    {
                        if (startingValue - valueLoss + heaviestItem.value > startingValue)
                        {
                            for (int x = 0; x < removedItems.Count; ++x)
                            {
                                if (startingWeight - weightLoss + heaviestItem.weight + removedItems[x].weight > maxCapacity)
                                {
                                    chosenItems.Remove(removedItems[x]);
                                }
                                else
                                {
                                    weightLoss -= removedItems[x].weight;
                                }
                            }
                            chosenItems.Add(heaviestItem);
                            startingWeight -= weightLoss - heaviestItem.weight;
                            startingValue -= valueLoss - heaviestItem.value;
                            lightIndex = j + 1;
                        }

                        removedItems.Clear();
                        weightLoss = 0;
                        valueLoss = 0;
                        break;
                    }
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
            
            for (int i = 2; i < numOfItems + 2; ++i)
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
                sb.Append(MakeLength(item.name, 23, '-'));
                sb.Append(" ");
                sb.Append(item.weight);
                sb.Append(" lbs. at $");
                sb.Append(item.value);
                sb.Append("\n");
            }

            Console.WriteLine(sb.ToString());
        }

        static string MakeLength(string s, int length, char spacerChar)
        {
            StringBuilder sb = new StringBuilder(s);
            sb.Append(" ");

            for (int i = 1; i < length - s.Length; ++i)
            {
                sb.Append(spacerChar);
            }

            return sb.ToString();
        }
    }
}
