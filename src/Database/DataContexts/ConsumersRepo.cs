using System.Linq;
using Database.Models;

namespace Database.DataContexts
{
	public class ConsumersRepo
	{
		private readonly ULearnDb db;

		public ConsumersRepo()
			: this(new ULearnDb())
		{
		}

		public ConsumersRepo(ULearnDb db)
		{
			this.db = db;
		}

		public LtiConsumer Find(string consumerKey)
		{
			return db.Consumers.SingleOrDefault(consumer => consumer.Key == consumerKey);
		}
	}
}