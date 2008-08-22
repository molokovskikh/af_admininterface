using Castle.ActiveRecord;

namespace Common.Web.Ui.Helpers
{
	public class ValidationHelper
	{
		public static bool IsInstanceHasValidationError(ActiveRecordValidationBase instance)
		{
			return instance.ValidationErrorMessages.Length > 0;
		}

		public static bool IsInstanceHasValidationError<T>(ActiveRecordValidationBase<T> instance) where T : class 
		{
			return instance.ValidationErrorMessages.Length > 0;
		}

		public static bool IsCollectionHasNotValideObject<T>(ActiveRecordValidationBase<T>[] collection) where T : class 
		{
			foreach (ActiveRecordValidationBase<T> instance in collection)
				if (IsInstanceHasValidationError(instance))
					return true;
			return false;
		}
	}
}
