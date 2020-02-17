using System;
using System.Threading;
using FluentAssertions;

namespace GraphQL.Client.Tests.Common.Helpers {
	public class CallbackTester<T> {
		private ManualResetEventSlim _callbackInvoked { get; } = new ManualResetEventSlim();

		/// <summary>
		/// The timeout for <see cref="ShouldHaveReceivedUpdate"/>. Defaults to 1 s
		/// </summary>
		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);

		/// <summary>
		/// Indicates that an update has been received since the last <see cref="Reset"/>
		/// </summary>
		public bool CallbackInvoked => _callbackInvoked.IsSet;
		/// <summary>
		/// The last payload which was received.
		/// </summary>
		public T LastPayload { get; private set; }

		public void Callback(T param) {
			LastPayload = param;
			_callbackInvoked.Set();
		}

		/// <summary>
		/// Asserts that a new update has been pushed to the <see cref="IObservable{T}"/> within the configured <see cref="Timeout"/> since the last <see cref="Reset"/>.
		/// If supplied, the <paramref name="assertPayload"/> action is executed on the submitted payload.
		/// </summary>
		/// <param name="assertPayload">action to assert the contents of the payload</param>
		public void CallbackShouldHaveBeenInvoked(Action<T> assertPayload = null, TimeSpan? timeout = null) {
			try {
				_callbackInvoked.Wait(timeout ?? Timeout).Should().BeTrue("because the callback method should have been invoked (timeout: {0} s)",
					(timeout ?? Timeout).TotalSeconds);

				assertPayload?.Invoke(LastPayload);
			}
			finally {
				Reset();
			}
		}

		/// <summary>
		/// Asserts that no new update has been pushed within the given <paramref name="millisecondsTimeout"/> since the last <see cref="Reset"/>
		/// </summary>
		/// <param name="millisecondsTimeout">the time in ms in which no new update must be pushed to the <see cref="IObservable{T}"/>. defaults to 100</param>
		public void CallbackShouldNotHaveBeenInvoked(TimeSpan? timeout = null) {
			if (!timeout.HasValue) timeout = TimeSpan.FromMilliseconds(100);
			try {
				_callbackInvoked.Wait(timeout.Value).Should().BeFalse("because the callback method should not have been invoked");
			}
			finally {
				Reset();
			}
		}

		/// <summary>
		/// Resets the tester class. Should be called before triggering the potential update
		/// </summary>
		public void Reset() {
			LastPayload = default(T);
			_callbackInvoked.Reset();
		}
	}
}