namespace Skyline.Automation.IAS
{
	using System.IO;
	using System.Text;

	public static class WrapText
	{
		public static string Wrap(this string text, int width)
		{
			var result = new StringBuilder();

			using (var sr = new StringReader(text))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					if (result.Length > 0) result.AppendLine();
					result.Append(WrapLine(line, width));
				}
			}

			return result.ToString();
		}

		public static string WrapLine(this string text, int width)
		{
			var result = new StringBuilder();
			var line = new StringBuilder();

			var words = text.Split(' ');

			foreach (var word in words)
			{
				if (line.Length + word.Length >= width)
				{
					if (result.Length > 0) result.AppendLine();
					result.Append(line.ToString());
					line.Clear();
				}

				if (line.Length > 0) line.Append(" ");
				line.Append(word);
			}

			if (line.Length > 0)
			{
				if (result.Length > 0) result.AppendLine();
				result.Append(line.ToString());
			}

			return result.ToString();
		}
	}
}
