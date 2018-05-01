using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
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
        static Stopwatch sw;
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
            sw = new Stopwatch();
            knapsackItems = new List<KnapsackItem>();
            chosenItems = new List<KnapsackItem>();
            seenItems = new List<KnapsackItem>();
            LoadList();
            Console.WriteLine($"\nMaximum Bag Capacity: {maxCapacity}\n\nStarting Item Pool:");
            //PrintItems(knapsackItems);
            FindBestItems();
            Console.WriteLine("Items I chose to put into knapsack:");
            sw.Stop();
            PrintItems(chosenItems);
            Console.WriteLine($"Total Weight: {KnapsackItem.Total(chosenItems, SpecifiedParameter.WEIGHT)} / {maxCapacity} lbs.");
            Console.WriteLine($"Total Value: ${KnapsackItem.Total(chosenItems, SpecifiedParameter.VALUE)}\n");
            Console.WriteLine($"Took {sw.ElapsedMilliseconds / 1000} Seconds ({sw.ElapsedMilliseconds} millis)\n");
        }

        static void FindBestItems()
        {
            List<KnapsackItem> weightedItems = KnapsackItem.SortBy(knapsackItems, SpecifiedParameter.WEIGHT);

            AddLightestItems(weightedItems);

            // Loop through lightest least valuable and keep removing them until you have enough room for heavy item
            // If at that point the addition of a heavy item would have an overall NET LOSS, don't do it, move on
            // If it is worth it to add the heavy item, then you will need to refresh the list and sort it again
            // Removed items should be added to a list that will be checked after the heavy items are checked
            // This way it will re-check the ones that were removed and there may be a large enough weight gap to slip it in
            // This process will repeat until nothing can be done
            // REMEMBER: Check and see if an item can be put in WITHOUT removing anything

            AddHeaviestItems(weightedItems);
        }

        static void AddLightestItems(List<KnapsackItem> weightedItems)
        {
            int currentWeight = KnapsackItem.Total(chosenItems, SpecifiedParameter.WEIGHT);

            for (int i = 0; i < weightedItems.Count; ++i)
            {
                KnapsackItem item = weightedItems[i];
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
        }

        static void AddHeaviestItems(List<KnapsackItem> weightedItems)
        {
            List<KnapsackItem> lightestByValue = KnapsackItem.SortBy(chosenItems, SpecifiedParameter.VALUE);
            List<KnapsackItem> removedItems = new List<KnapsackItem>();
            List<KnapsackItem> addedItems = new List<KnapsackItem>();
            int startingValue = KnapsackItem.Total(chosenItems, SpecifiedParameter.VALUE);
            int startingWeight = KnapsackItem.Total(chosenItems, SpecifiedParameter.WEIGHT);
            int chosenCount = chosenItems.Count;
            int weightLoss = 0;
            int valueLoss = 0;
            int lightIndex = 0;
            for (int i = weightedItems.Count - 1; i >= 0; --i)
            {
                KnapsackItem heaviestItem = weightedItems[i];
                if (!chosenItems.Contains(heaviestItem))
                {
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
                                        lightestByValue.Remove(removedItems[x]);
                                    }
                                    else
                                    {
                                        weightLoss -= removedItems[x].weight;
                                    }
                                }
                                //Console.WriteLine("Cerchunk!");
                                chosenItems.Add(heaviestItem);
                                startingValue = KnapsackItem.Total(chosenItems, SpecifiedParameter.VALUE);
                                startingWeight = KnapsackItem.Total(chosenItems, SpecifiedParameter.WEIGHT);
                            }
                            else
                            {
                                addedItems.Add(heaviestItem);
                                int valueAdded = heaviestItem.value;
                                int weightAdded = heaviestItem.weight;
                                for (int x = i - 1; x >= 0; --x)
                                {
                                    if (startingWeight - weightLoss + weightedItems[x].weight + heaviestItem.weight <= maxCapacity)
                                    {
                                        addedItems.Add(weightedItems[x]);
                                        valueAdded += weightedItems[x].value;
                                        weightAdded += weightedItems[x].weight;
                                    }
                                    else
                                    {
                                        valueAdded -= addedItems.ElementAt(addedItems.Count - 1).value;
                                        weightAdded -= addedItems.ElementAt(addedItems.Count - 1).weight;
                                        if (startingValue + valueAdded - valueLoss > startingValue)
                                        {
                                            for (int y = 0; y < removedItems.Count; ++y)
                                            {
                                                if (startingWeight - weightLoss + weightAdded + removedItems[y].weight > maxCapacity)
                                                {
                                                    chosenItems.Remove(removedItems[y]);
                                                    lightestByValue.Remove(removedItems[y]);
                                                }
                                                else
                                                {
                                                    weightLoss -= removedItems[y].weight;
                                                }
                                            }
                                            Console.WriteLine("Wow! more value!");
                                        }
                                        else
                                        {
                                            //Console.WriteLine("Not enough value!");
                                            addedItems.Clear();
                                        }
                                        foreach (KnapsackItem item in addedItems)
                                        {
                                            chosenItems.Add(item);
                                        }
                                        break;
                                    }
                                }
                            }
                            weightLoss = 0;
                            valueLoss = 0;
                            removedItems.Clear();
                            break;
                        }
                    }
                    weightLoss = 0;
                    valueLoss = 0;
                }
            }
        }

        static void LoadList()
        {
            ParseItemsFromFile(GetFilePathFromUser());
            sw.Start();
            //FillListRandom(knapsackItems, 1000000);
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

        static void FillListRandom(List<KnapsackItem> items, int amount, int minimum = 50, int maximum = 200)
        {
            Random rand = new Random();
            maxCapacity = (rand.Next() % (maximum - minimum + 1)) + minimum * (int)(amount * 0.000001f + 1.0f);
            for (int i = 0; i < amount; ++i)
            {
                int weight = (rand.Next() % (maxCapacity / 2)) + 1;
                int value = (int)(weight * (rand.NextDouble() * 10.0f + 1.0f));
                KnapsackItem item = new KnapsackItem($"Item {i + 1}", weight, value);
                items.Add(item);
            }
        }
    }
}
