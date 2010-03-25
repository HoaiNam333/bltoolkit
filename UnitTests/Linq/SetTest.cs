﻿using System;
using System.Linq;
using BLToolkit.Data.DataProvider;
using Data.Linq.Model;
using NUnit.Framework;

namespace Data.Linq
{
	[TestFixture]
	public class SetTest : TestBase
	{
		[Test]
		public void Concat1()
		{
			var expected =
				(from p in Parent where p.ParentID == 1 select p).Concat(
				(from p in Parent where p.ParentID == 2 select p));

			ForEachProvider(db => AreEqual(expected, 
				(from p in db.Parent where p.ParentID == 1 select p).Concat(
				(from p in db.Parent where p.ParentID == 2 select p))));
		}

		[Test]
		public void Concat2()
		{
			var expected =
				(from p in Parent where p.ParentID == 1 select p).Concat(
				(from p in Parent where p.ParentID == 2 select p)).Concat(
				(from p in Parent where p.ParentID == 4 select p));

			ForEachProvider(db => AreEqual(expected, 
				(from p in db.Parent where p.ParentID == 1 select p).Concat(
				(from p in db.Parent where p.ParentID == 2 select p)).Concat(
				(from p in db.Parent where p.ParentID == 4 select p))));
		}

		[Test]
		public void Concat3()
		{
			var expected =
				(from p in Parent where p.ParentID == 1 select p).Concat(
				(from p in Parent where p.ParentID == 2 select p).Concat(
				(from p in Parent where p.ParentID == 4 select p)));

			ForEachProvider(db => AreEqual(expected, 
				(from p in db.Parent where p.ParentID == 1 select p).Concat(
				(from p in db.Parent where p.ParentID == 2 select p).Concat(
				(from p in db.Parent where p.ParentID == 4 select p)))));
		}

		[Test]
		public void Concat4()
		{
			var expected =
				(from c in Child where c.ParentID == 1 select c).Concat(
				(from c in Child where c.ParentID == 3 select new Child { ParentID = c.ParentID, ChildID = c.ChildID + 1000}).
				Where(c => c.ChildID != 1032));

			ForEachProvider(db => AreEqual(expected, 
				(from c in db.Child where c.ParentID == 1 select c).Concat(
				(from c in db.Child where c.ParentID == 3 select new Child { ParentID = c.ParentID, ChildID = c.ChildID + 1000})).
				Where(c => c.ChildID != 1032)));
		}

		[Test]
		public void Concat5()
		{
			var expected =
				(from c in Child where c.ParentID == 1 select c).Concat(
				(from c in Child where c.ParentID == 3 select new Child { ChildID = c.ChildID + 1000}).
				Where(c => c.ChildID != 1032));

			ForEachProvider(new[] { ProviderName.DB2, ProviderName.Informix }, db => AreEqual(expected, 
				(from c in db.Child where c.ParentID == 1 select c).Concat(
				(from c in db.Child where c.ParentID == 3 select new Child { ChildID = c.ChildID + 1000})).
				Where(c => c.ChildID != 1032)));
		}

		[Test]
		public void Concat6()
		{
			var expected =
				Child.Where(c => c.GrandChildren.Count == 2).Concat(Child.Where(c => c.GrandChildren.Count() == 3));

			ForEachProvider(new[] { ProviderName.SqlCe }, db => AreEqual(expected, 
				db.Child.Where(c => c.GrandChildren.Count == 2).Concat(db.Child.Where(c => c.GrandChildren.Count() == 3))));
		}

		[Test]
		public void Concat7()
		{
			using (var db = new NorthwindDB())
				AreEqual(
					   Customer.Where(c => c.Orders.Count <= 1).Concat(   Customer.Where(c => c.Orders.Count > 1)),
					db.Customer.Where(c => c.Orders.Count <= 1).Concat(db.Customer.Where(c => c.Orders.Count > 1)));
		}

		[Test]
		public void Except()
		{
			ForEachProvider(db => AreEqual(
				   Child.Except(   Child.Where(p => p.ParentID == 3)),
				db.Child.Except(db.Child.Where(p => p.ParentID == 3))));
		}

		[Test]
		public void Intersect()
		{
			ForEachProvider(db => AreEqual(
				   Child.Intersect(   Child.Where(p => p.ParentID == 3)),
				db.Child.Intersect(db.Child.Where(p => p.ParentID == 3))));
		}

		[Test]
		public void Any1()
		{
			ForEachProvider(db => AreEqual(
				   Parent.Where(p =>    Child.Where(c => c.ParentID == p.ParentID).Any(c => c.ParentID > 3)),
				db.Parent.Where(p => db.Child.Where(c => c.ParentID == p.ParentID).Any(c => c.ParentID > 3))));
		}

		[Test]
		public void Any2()
		{
			ForEachProvider(db => AreEqual(
				   Parent.Where(p =>    Child.Where(c => c.ParentID == p.ParentID).Any()),
				db.Parent.Where(p => db.Child.Where(c => c.ParentID == p.ParentID).Any())));
		}

		[Test]
		public void Any3()
		{
			ForEachProvider(db => AreEqual(
				   Parent.Where(p => p.Children.Any(c => c.ParentID > 3)),
				db.Parent.Where(p => p.Children.Any(c => c.ParentID > 3))));
		}

		[Test]
		public void Any4()
		{
			ForEachProvider(db => AreEqual(
				   Parent.Where(p => p.Children.Any()),
				db.Parent.Where(p => p.Children.Any())));
		}

		[Test]
		public void Any5()
		{
			ForEachProvider(db => AreEqual(
				   Parent.Where(p => p.Children.Any(c => c.GrandChildren.Any(g => g.ParentID > 3))),
				db.Parent.Where(p => p.Children.Any(c => c.GrandChildren.Any(g => g.ParentID > 3)))));
		}

		[Test]
		public void Any6()
		{
			ForEachProvider(db => Assert.AreEqual(
				   Child.Any(c => c.ParentID > 3),
				db.Child.Any(c => c.ParentID > 3)));
		}

		[Test]
		public void Any7()
		{
			ForEachProvider(db => Assert.AreEqual(Child.Any(), db.Child.Any()));
		}

		[Test]
		public void Any8()
		{
			ForEachProvider(db => AreEqual(
				from p in    Parent select    Child.Select(c => c.Parent).Any(c => c == p),
				from p in db.Parent select db.Child.Select(c => c.Parent).Any(c => c == p)));
		}

		[Test]
		public void All1()
		{
			ForEachProvider(db => AreEqual(
				   Parent.Where(p =>    Child.Where(c => c.ParentID == p.ParentID).All(c => c.ParentID > 3)),
				db.Parent.Where(p => db.Child.Where(c => c.ParentID == p.ParentID).All(c => c.ParentID > 3))));
		}

		[Test]
		public void All2()
		{
			ForEachProvider(db => AreEqual(
				   Parent.Where(p => p.Children.All(c => c.ParentID > 3)),
				db.Parent.Where(p => p.Children.All(c => c.ParentID > 3))));
		}

		[Test]
		public void All3()
		{
			ForEachProvider(db => AreEqual(
				   Parent.Where(p => p.Children.All(c => c.GrandChildren.All(g => g.ParentID > 3))),
				db.Parent.Where(p => p.Children.All(c => c.GrandChildren.All(g => g.ParentID > 3)))));
		}

		[Test]
		public void All4()
		{
			ForEachProvider(db => Assert.AreEqual(
				   Child.All(c => c.ParentID > 3),
				db.Child.All(c => c.ParentID > 3)));
		}

		[Test]
		public void All5()
		{
			int n = 3;

			ForEachProvider(db => Assert.AreEqual(
				   Child.All(c => c.ParentID > n),
				db.Child.All(c => c.ParentID > n)));
		}

		[Test]
		public void SubQueryAllAny()
		{
			ForEachProvider(db => AreEqual(
				from c in    Parent
				where    Child.Where(o => o.Parent == c).All(o =>    Child.Where(e => o == e).Any(e => e.ChildID > 10))
				select c,
				from c in db.Parent
				where db.Child.Where(o => o.Parent == c).All(o => db.Child.Where(e => o == e).Any(e => e.ChildID > 10))
				select c));
		}

		[Test]
		public void AllNestedTest()
		{
			using (var db = new NorthwindDB())
				AreEqual(
					from c in    Customer
					where    Order.Where(o => o.Customer == c).All(o =>    Employee.Where(e => o.Employee == e).Any(e => e.FirstName.StartsWith("A")))
					select c,
					from c in db.Customer
					where db.Order.Where(o => o.Customer == c).All(o => db.Employee.Where(e => o.Employee == e).Any(e => e.FirstName.StartsWith("A")))
					select c);
		}

		[Test]
		public void ComplexAllTest()
		{
			using (var db = new NorthwindDB())
				AreEqual(
					from o in Order
					where
						Customer.Where(c => c == o.Customer).All(c => c.CompanyName.StartsWith("A")) ||
						Employee.Where(e => e == o.Employee).All(e => e.FirstName.EndsWith("t"))
					select o,
					from o in db.Order
					where
						db.Customer.Where(c => c == o.Customer).All(c => c.CompanyName.StartsWith("A")) ||
						db.Employee.Where(e => e == o.Employee).All(e => e.FirstName.EndsWith("t"))
					select o);
		}

		[Test]
		public void Contains1()
		{
			ForEachProvider(db => AreEqual(
				from p in    Parent select    Child.Select(c => c.Parent).Contains(p),
				from p in db.Parent select db.Child.Select(c => c.Parent).Contains(p)));
		}

		[Test]
		public void Contains2()
		{
			ForEachProvider(db => AreEqual(
				from p in    Parent select    Child.Select(c => c.ParentID).Contains(p.ParentID),
				from p in db.Parent select db.Child.Select(c => c.ParentID).Contains(p.ParentID)));
		}

		[Test]
		public void Contains3()
		{
			ForEachProvider(db => AreEqual(
				from p in    Parent where    Child.Select(c => c.Parent).Contains(p) select p,
				from p in db.Parent where db.Child.Select(c => c.Parent).Contains(p) select p));
		}

		[Test]
		public void Contains4()
		{
			ForEachProvider(db => AreEqual(
				from p in    Parent where    Child.Select(c => c.ParentID).Contains(p.ParentID) select p,
				from p in db.Parent where db.Child.Select(c => c.ParentID).Contains(p.ParentID) select p));
		}

		[Test]
		public void Contains5()
		{
			ForEachProvider(db => AreEqual(
				from p in    Parent where    Child.Select(c => c.ParentID).Contains(p.ParentID + 1) select p,
				from p in db.Parent where db.Child.Select(c => c.ParentID).Contains(p.ParentID + 1) select p));
		}

		[Test]
		public void Contains6()
		{
			var n = 1;

			ForEachProvider(db => AreEqual(
				from p in    Parent where    Child.Select(c => c.ParentID).Contains(p.ParentID + n) select p,
				from p in db.Parent where db.Child.Select(c => c.ParentID).Contains(p.ParentID + n) select p));
		}

		[Test]
		public void Contains7()
		{
			ForEachProvider(db => Assert.AreEqual(
				   Child.Select(c => c.ParentID).Contains(11),
				db.Child.Select(c => c.ParentID).Contains(11)));
		}

		[Test]
		public void Contains8()
		{
			var arr = new[] { GrandChild[0], GrandChild[1] };

			ForEachProvider(db => AreEqual(
				from p in Parent
				join ch in Child on p.ParentID equals ch.ParentID
				join gc in GrandChild on ch.ChildID equals gc.ChildID
				where arr.Contains(gc)
				select p,
				from p in db.Parent
				join ch in db.Child on p.ParentID equals ch.ParentID
				join gc in db.GrandChild on ch.ChildID equals gc.ChildID
				where arr.Contains(gc)
				select p));
		}

		[Test]
		public void Contains9()
		{
			var arr = new[] { Parent1[0], Parent1[1] };

			ForEachProvider(db => AreEqual(
				from p in    Parent1 where arr.Contains(p) select p,
				from p in db.Parent1 where arr.Contains(p) select p));
		}

		[Test]
		public void Union1()
		{
			ForEachProvider(db => AreEqual(
				(from g  in    GrandChild join ch in    Child  on g.ChildID   equals ch.ChildID select ch).Union(
				(from ch in    Child      join p  in    Parent on ch.ParentID equals p.ParentID select ch)),
				(from g  in db.GrandChild join ch in db.Child  on g.ChildID   equals ch.ChildID select ch).Union(
				(from ch in db.Child      join p  in db.Parent on ch.ParentID equals p.ParentID select ch))));
		}
	}
}