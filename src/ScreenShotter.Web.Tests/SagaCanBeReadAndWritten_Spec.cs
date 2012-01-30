using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Testing;
using Magnum.StateMachine;
using NHibernate;
using NUnit.Framework;
using ScreenShotter.Web.App_Start;
using StructureMap;

namespace ScreenShotter.Web.Tests
{
	[TestFixture]
	public class SagaCanBeReadAndWritten_Spec
	{
		ISession session;
		private IContainer container;

		[SetUp]
		public void get_Container()
		{
			container = Bootstrapper.CreateContainer("testsaga.db");
			var fac = container.GetInstance<ISessionFactory>();
			fac.OpenSession();
		}

		[TearDown]
		public void teardown()
		{
			session.Dispose();
			container.Dispose();
		}

		[Test]
		public void CanCorrectlyMapEmployee()
		{
			new PersistenceSpecification<WebShotSaga>(session)
				.CheckProperty(c => c.CorrelationId, Guid.NewGuid())
				.VerifyTheMappings();
		}
	}
}
