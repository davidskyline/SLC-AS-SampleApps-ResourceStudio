namespace Script
{
	using System;

	internal class OutputData
	{
		public Guid ResourceId { get; set; }

		public bool IsSuccess { get; set; }

		public string ErrorReason {get; set; }
	}
}
