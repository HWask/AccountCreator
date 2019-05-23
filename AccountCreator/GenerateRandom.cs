using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountCreator
{
    public class GenerateRandom
    {
        private static Random random = new Random((int)DateTime.Now.Ticks);

        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch = ' ';
            int choice;

            for (int i = 0; i < size; i++)
            {
                choice = random.Next(1, 3);

                switch (choice)
                {
                    case 1:
                        ch = Convert.ToChar(random.Next(97, 122));
                        break;
                    case 2:
                        ch = Convert.ToChar(random.Next(48, 57));
                        break;
                    case 3:
                        ch = Convert.ToChar(random.Next(65, 90));
                        break;
                }

                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static string RandomNumber(int min, int max)
        {
            return random.Next(min, max).ToString();
        }

        public static string RandomStringOnlyChars(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch = ' ';

            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(random.Next(97, 122));
                builder.Append(ch);
            }

            return builder.ToString();
        }
    }
}
