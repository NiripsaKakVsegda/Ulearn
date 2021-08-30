using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Database.Extensions;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common.Extensions;

namespace Database.Repos
{
	public class TextsRepo : ITextsRepo
	{
		private readonly UlearnDb db;
		public const int MaxTextSize = 50000;

		public TextsRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task<TextBlob> AddText(string text)
		{
			if (text == null)
				return new TextBlob
				{
					Hash = null,
					Text = null
				};

			if (text.Length > MaxTextSize)
				text = text.Substring(0, MaxTextSize);

			if (text.Contains('\0'))
				text = text.Replace("\0", ""); // postgres не поддерживает \0 в строках

			var hash = GetHash(text);
			var blob = await db.Texts.FindAsync(hash);
			if (blob != null)
				return blob;

			blob = new TextBlob
			{
				Hash = hash,
				Text = text
			};
			db.AddOrUpdate(blob, b => b.Hash == hash);

			try
			{
				await db.SaveChangesAsync().ConfigureAwait(false);
			}
			catch (DbUpdateException)
			{
				// It's ok, just tried to insert text with hash which already exists, try to find it
				if (!db.Texts.AsNoTracking().Any(t => t.Hash == hash))
					throw;
				db.Entry(blob).State = EntityState.Unchanged;
			}

			return blob;
		}

		private static string GetHash(string text)
		{
			var byteArray = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(text));
			return BitConverter.ToString(byteArray).Replace("-", "");
		}

		public async Task<TextBlob> GetText(string hash)
		{
			if (hash == null)
				return new TextBlob
				{
					Hash = null,
					Text = null
				};
			return await db.Texts.FindAsync(hash);
		}

		public async Task<Dictionary<string, string>> GetTextsByHashes(IEnumerable<string> hashes)
		{
			return (await db.Texts
				.Where(t => hashes.Contains(t.Hash))
				.ToListAsync())
				.ToDictSafe(t => t.Hash, t => t.Text);
		}
	}
}