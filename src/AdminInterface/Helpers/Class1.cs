using System;
using System.Text;

namespace AdminInterface.Helpers
{
	public class TextUtil
	{
		/// <summary>
		/// Род единицы измерения.
		/// </summary>
		public enum WordGender
		{
			Masculine, // мужской
			Feminine, // женский
			Neuter // средний
		} ;

		// Строковые представления чисел
		private const string number0 = "ноль";

		private static readonly string[][] _number1_2 =
			{
				new[] {"один ", "два "},
				new[] {"одна ", "две "},
				new[] {"одно ", "два "}
			};

		private static readonly string[] _number3_9 = {"три ", "четыре ", "пять ", "шесть ", "семь ", "восемь ", "девять "};

		private static readonly string[] _number10_19 =
			{
				"десять ", "одиннадцать ", "двенадцать ", "тринадцать ", "четырнадцать ", "пятнадцать ",
				"шестнадцать ", "семнадцать ", "восемнадцать ", "девятнадцать "
			};

		private static readonly string[] _number20_90 = {
		                                                	"двадцать ", "тридцать ", "сорок ", "пятьдесят ", "шестьдесят ",
		                                                	"семьдесят ", "восемьдесят ", "девяносто "
		                                                };

		private static readonly string[] _number100_900 = {
		                                                  	"сто ", "двести ", "триста ", "четыреста ", "пятьсот ", "шестьсот "
		                                                  	, "семьсот ", "восемьсот ", "девятьсот "
		                                                  };

		private static readonly string[,] _ternaries =
			{
				{"тысяча ", "тысячи ", "тысяч "},
				{"миллион ", "миллиона ", "миллионов "},
				{"миллиард ", "миллиарда ", "миллиардов "},
				{"триллион ", "триллиона ", "триллионов "},
				{"биллион ", "биллиона ", "биллионов "}
			};

		private static readonly WordGender[] _ternaryGenders =
			{
				WordGender.Feminine, // тысяча - женский
				WordGender.Masculine, // миллион - мужской
				WordGender.Masculine, // миллиард - мужской
				WordGender.Masculine, // триллион - мужской
				WordGender.Masculine // биллион - мужской
			};

		// Функция преобразования 3-значного числа, заданного в виде строки,
		// с учетом рода (мужской, женский или средний).
		// Род учитывается для корректного формирования концовки:
		// "один" (рубль) или "одна" (тысяча)
		private static string TernaryToString(long ternary, WordGender gender)
		{
			var s = new StringBuilder(100);
			;
			// учитываются только последние 3 разряда, т.е. 0..999    
			long t = ternary%1000;
			var digit2 = (int) (t/100);
			var digit1 = (int) ((t%100)/10);
			var digit0 = (int) (t%10);
			// сотни
			if (digit2 > 0)
				s.Append(_number100_900[digit2 - 1]);
			if (digit1 > 1)
			{
				s.Append(_number20_90[digit1 - 2]);
				if (digit0 >= 3)
					s.Append(_number3_9[digit0 - 3]);
				else if (digit0 > 0)
					s.Append(_number1_2[(int) gender][digit0 - 1]); // Были переставлены местами
			}
			else if (digit1 == 1)
				s.Append(_number10_19[digit0]);
			else
			{
				if (digit0 >= 3)
					s.Append(_number3_9[digit0 - 3]);
				else if (digit0 > 0)
				{
					var i = (int) gender;
					s.Append(_number1_2[i][digit0 - 1]); // Были переставлены местами
				}
				//else if(s == string.Empty) // Приводило к появлению глюка типа "миллионов тысяч".
				//    s += number0 + " ";
			}
			return s.ToString().TrimEnd();
		}

		//
		private static string TernaryToString(long value, byte ternaryIndex)
		{
			long ternary = value;
			for (byte i = 0; i < ternaryIndex; i++)
				ternary /= 1000;
			if (ternary == 0)
				return string.Empty;

			ternaryIndex--;
			string s = TernaryToString(ternary, _ternaryGenders[ternaryIndex]) + " ";
			if (ternary%1000 > 0)
				s += _ternaries[ternaryIndex, (int) GetWordMode(ternary)];
			return s;
		}

		public static string FirstUpper(string str)
		{
			return str[0].ToString().ToUpper() + str.Substring(1, str.Length - 1);
		}


		public static string NumToString(double sum)
		{
			return NumToString(sum, false, false, true);
		}

		/// <summary>
		/// Функция возвращает число рублей и копеек прописью.
		/// </summary>
		public static string NumToString(double sum,
		                                 bool shortHigh, // false
		                                 bool shortLow, // false
		                                 bool digitLow) // true
		{
			var r = (long) sum;
			var c = (long) ((Math.Round((sum - r)*100, 0)));
			string result = string.Format("{0} {1} {2} {3}",
			                              NumberToString(r, GetGender(true)),
			                              shortHigh
			                              	? GetShortName(true)
			                              	: GetName(GetWordMode(r), true),
			                              digitLow
			                              	? string.Format("{0:d2}", c)
			                              	: NumberToString(c, GetGender(false)),
			                              shortLow
			                              	? GetShortName(false)
			                              	: GetName(GetWordMode(c), false));
			result = result.Trim();
			int len;
			do
			{
				len = result.Length;
				result = result.Replace("  ", " ");
			} while (len != result.Length);
			return result;
		}

		// варианты написания рублей
		private static readonly string[] roubles = {"рубль", "рубля", "рублей"};
		// варианты написания копеек
		private static readonly string[] copecks = {"копейка", "копейки", "копеек"};

		protected static WordGender GetGender(bool high)
		{
			return high ? WordGender.Masculine : WordGender.Feminine;
		}

		// Функция возвращает наименование денежной единицы в соответствующей форме 
		//  (1) рубль / (2) рубля / (5) рублей
		protected static string GetName(WordMode wordMode, bool high)
		{
			return high ? roubles[(int) wordMode] : copecks[(int) wordMode];
		}

		// Функция возвращает сокращенное наименование денежной единицы 
		protected static string GetShortName(bool high)
		{
			return high ? "руб." : "коп.";
		}

		/// <summary>
		/// Функция возвращает число прописью с учетом рода единицы измерения.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="gender"></param>
		/// <returns></returns>
		protected static string NumberToString(long value, WordGender gender)
		{
			if (value <= 0)
				return number0;

			return TernaryToString(value, 5) +
			       TernaryToString(value, 4) +
			       TernaryToString(value, 3) +
			       TernaryToString(value, 2) +
			       TernaryToString(value, 1) +
			       TernaryToString(value, gender);
		}

		/// <summary>
		/// Варианты написания единицы измерения.
		/// </summary>
		public enum WordMode
		{
			Mode1, // рубль
			Mode2_4, // рубля
			Mode0_5 // рублей
		} ;

		/// <summary>
		/// Определение варианта написания единицы измерения по 3-х значному 
		/// числу.
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		protected static WordMode GetWordMode(long number)
		{
			// достаточно проверять только последние 2 цифры,
			// т.к. разные падежи единицы измерения раскладываются
			// 0 рублей, 1 рубль, 2-4 рубля, 5-20 рублей, 
			// дальше - аналогично первому десятку        
			int digit1 = (int) (number%100)/10;
			var digit0 = (int) (number%10);
			if (digit1 == 1)
				return WordMode.Mode0_5;
			if (digit0 == 1)
				return WordMode.Mode1;
			if (2 <= digit0 && digit0 <= 4)
				return WordMode.Mode2_4;
			return WordMode.Mode0_5;
		}
	}
}
