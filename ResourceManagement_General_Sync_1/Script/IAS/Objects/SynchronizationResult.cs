namespace Script.IAS
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class SynchronizationResult
	{
		private readonly string reference;

		private readonly Dictionary<string, SynchronizationItem> deSyncItemsByName;

		public SynchronizationResult(string reference)
		{
			if (string.IsNullOrEmpty(reference))
			{
				throw new ArgumentNullException(nameof(reference));
			}

			this.reference = reference;

			deSyncItemsByName = new Dictionary<string, SynchronizationItem>();
		}

		public string Reference => reference;

		public bool IsSynchronized => deSyncItemsByName.Count == 0;

		public IReadOnlyCollection<SynchronizationItem> DeSyncedItems => deSyncItemsByName.Values;

		public void AddDeSyncDetail(string name, string detail)
		{
			if (!deSyncItemsByName.TryGetValue(name, out var item))
			{
				item = new SynchronizationItem
				{
					Name = name,
					Details = new List<string>(),
				};

				deSyncItemsByName.Add(name, item);
			}

			item.Details.Add(detail);
		}
	}
}
