﻿using NUnit.Framework;
using Stylet;
using System;

namespace StyletUnitTests;

[TestFixture]
public class PropertyChangedExtensionsTests
{
    private class NotifyingClass : PropertyChangedBase
    {
        private string _foo;
        public string Foo
        {
            get => this._foo;
            set => this.SetAndNotify(ref this._foo, value);
        }

        private string _bar;
        public string Bar
        {
            get => this._bar;
            set => this.SetAndNotify(ref this._bar, value);
        }

        public void NotifyAll()
        {
            this.NotifyOfPropertyChange(string.Empty);
        }
    }

    private class BindingClass
    {
        public string LastFoo;

        public IEventBinding BindStrong(NotifyingClass notifying)
        {
            // Must make sure the compiler doesn't generate an inner class for this, otherwise we're not testing the right thing
            return notifying.Bind(x => x.Foo, (o, e) => this.LastFoo = e.NewValue);
        }
    }

    [Test]
    public void StrongBindingBinds()
    {
        string newVal = null;
        var c1 = new NotifyingClass();
        c1.Bind(x => x.Foo, (o, e) => newVal = e.NewValue);
        c1.Foo = "bar";

        Assert.AreEqual("bar", newVal);
    }

    [Test]
    public void StrongBindingIgnoresOtherProperties()
    {
        string newVal = null;
        var c1 = new NotifyingClass();
        c1.Bind(x => x.Bar, (o, e) => newVal = e.NewValue);
        c1.Foo = "bar";

        Assert.AreEqual(null, newVal);
    }

    [Test]
    public void StrongBindingListensToEmptyString()
    {
        string newVal = null;
        var c1 = new NotifyingClass
        {
            Bar = "bar"
        };
        c1.Bind(x => x.Bar, (o, e) => newVal = e.NewValue);
        c1.NotifyAll();

        Assert.AreEqual("bar", newVal);
    }

    [Test]
    public void StrongBindingDoesNotRetainNotifier()
    {
        WeakReference Test(out IEventBinding binder)
        {
            var binding = new BindingClass();
            var notifying = new NotifyingClass();
            // Retain the IPropertyChangedBinding, in case that causes NotifyingClass to be retained
            binder = binding.BindStrong(notifying);
            return new WeakReference(notifying);
        }

        WeakReference weakNotifying = Test(out IEventBinding retained);

        GC.Collect();

        Assert.IsFalse(weakNotifying.IsAlive);
        GC.KeepAlive(retained);
    }

    [Test]
    public void StrongBindingPassesTarget()
    {
        var c1 = new NotifyingClass();
        object sender = null;
        c1.Bind(x => x.Foo, (o, e) => sender = o);
        c1.Foo = "foo";
        Assert.AreEqual(c1, sender);
    }

    [Test]
    public void StrongBindingUnbinds()
    {
        string newVal = null;
        var c1 = new NotifyingClass();
        IEventBinding binding = c1.Bind(x => x.Bar, (o, e) => newVal = e.NewValue);
        binding.Unbind();
        c1.Bar = "bar";

        Assert.AreEqual(null, newVal);
    }

    [Test]
    public void BindAndInvokeInvokes()
    {
        var c1 = new NotifyingClass()
        {
            Foo = "FooVal",
        };
        PropertyChangedExtendedEventArgs<string> ea = null;
        c1.BindAndInvoke(s => s.Foo, (o, e) => ea = e);

        Assert.NotNull(ea);
        Assert.AreEqual("Foo", ea.PropertyName);
        Assert.AreEqual("FooVal", ea.NewValue);
    }
}
