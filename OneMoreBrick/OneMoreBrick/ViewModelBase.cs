﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace OneMoreBrick {
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly List<ViewModelBase> childViewModels = new List<ViewModelBase>();

        protected void AddChildViewModel(ViewModelBase childViewModel) {
            childViewModels.Add(childViewModel);
        }

        /// <summary>Consider using SetProperty!</summary>
        private void NotifyPropertyChanged(string name) {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if (propertyChanged != null) {
                propertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>Consider using SetProperty!</summary>
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> fireNotifyProperty) {
            MemberExpression body = (MemberExpression)fireNotifyProperty.Body;
            NotifyPropertyChanged(body.Member.Name);
        }

        private bool SetProperty<T>(T value, ref T property, string fireNotifyProperty) where T : struct {
            Debug.Assert(Char.IsUpper(fireNotifyProperty[0]));
            if (property.Equals(value)) {
                return false;
            }
            property = value;
            NotifyPropertyChanged(fireNotifyProperty);
            return true;
        }

        protected bool SetDoubleProperty(double value, ref double property, Expression<Func<double>> fireNotifyProperty) {
            if (!DoubleUtility.AreClose(value, property)) {
                return SetProperty(value, ref property, fireNotifyProperty);
            }
            return false;
        }

        protected bool SetProperty<T>(T value, ref T property, Expression<Func<T>> fireNotifyProperty) where T : struct {
            MemberExpression body = (MemberExpression)fireNotifyProperty.Body;
            return SetProperty(value, ref property, body.Member.Name);
        }

        private bool SetPropertyClass<T>(T value, ref T property, string fireNotifyProperty) where T : class {
            if (value == null && property == null) {
                return false;
            }

            if (property != null && property.Equals(value)) {
                return false;
            }
            property = value;
            NotifyPropertyChanged(fireNotifyProperty);
            return true;
        }

        private bool SetPropertyNullable<T>(T value, ref T property, string fireNotifyProperty) {
            if (property.Equals(value)) {
                return false;
            }
            property = value;
            NotifyPropertyChanged(fireNotifyProperty);
            return true;
        }

        protected bool SetPropertyNullable<T>(T value, ref T property, Expression<Func<T>> fireNotifyProperty) {
            MemberExpression body = (MemberExpression)fireNotifyProperty.Body;
            return SetPropertyNullable(value, ref property, body.Member.Name);
        }

        protected bool SetPropertyClass<T>(T value, ref T property, Expression<Func<T>> fireNotifyProperty) where T : class {
            MemberExpression body = (MemberExpression)fireNotifyProperty.Body;
            return SetPropertyClass(value, ref property, body.Member.Name);
        }

        ~ViewModelBase() {
            Dispose(false);
        }

        protected virtual void DisposeViewModel() {
            childViewModels.ForEach(x => x.Dispose());
            childViewModels.Clear();
        }

        protected bool Disposed;

        private void Dispose(bool disposing) {
            if (disposing && !Disposed) {
                Disposed = true;
                DisposeViewModel();
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
            childViewModels.ForEach(x => x.Dispose());
        }

        /// <summary>Helper. Calls Dispose(), and sets object to null.</summary>
        protected static void Dispose<T>(ref T obj) where T : class, IDisposable {
            if (obj != null) {
                obj.Dispose();
                obj = null;
            }
        }
    }
}
