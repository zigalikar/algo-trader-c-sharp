using System.ComponentModel;
using System.Collections.Generic;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard.Model.Views
{
    public class ATDataErrorInfo : PropertyChangedBase, IDataErrorInfo
    {
        private readonly object _context;
        public Dictionary<string, string> ValidationErrors { get; private set; }

        private bool _validationResult;
        public bool ValidationResult { get => _validationResult; private set => Set(ref _validationResult, value); }

        private Dictionary<string, bool> _validation = new Dictionary<string, bool>();

        public string Error => null;
        public string this[string columnName]
        {
            get
            {
                var val = _context.GetType().GetProperty(columnName).GetValue(_context);
                if (val == null || string.IsNullOrWhiteSpace(val.ToString()))
                {
                    _validation[columnName] = true;
                    ValidationResult = _validation.Count == 0;
                    return ValidationErrors[columnName];
                }
                else
                {
                    _validation.Remove(columnName);
                    ValidationResult = _validation.Count == 0;
                }

                return string.Empty;
            }
        }

        public ATDataErrorInfo(object context, Dictionary<string, string> validationErrors)
        {
            _context = context;
            ValidationErrors = validationErrors;
        }
    }
}
