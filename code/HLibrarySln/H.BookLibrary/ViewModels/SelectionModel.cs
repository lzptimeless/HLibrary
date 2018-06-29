using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.BookLibrary.ViewModels
{
    public class SelectionModel<T> : BindableBase
    {
        public SelectionModel(string text, T value)
        {
            Text = text;
            Value = value;
        }

        #region Value
        /// <summary>
        /// Property name of <see cref="Value"/>
        /// </summary>
        public const string ValuePropertyName = "Value";
        private T _value;
        /// <summary>
        /// Get or set <see cref="Value"/>
        /// </summary>
        public T Value
        {
            get { return _value; }
            set
            {
                if (object.Equals(_value, value)) return;

                _value = value;
                RaisePropertyChanged(ValuePropertyName);
            }
        }
        #endregion

        #region Text
        /// <summary>
        /// Property name of <see cref="Text"/>
        /// </summary>
        public const string TextPropertyName = "Text";
        private string _text;
        /// <summary>
        /// Get or set <see cref="Text"/>
        /// </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value) return;

                _text = value;
                RaisePropertyChanged(TextPropertyName);
            }
        }
        #endregion

        public override string ToString()
        {
            return $"{Value} | {Text}";
        }
    }
}
