﻿using Cleipnir.ResilientFunctions.CoreRuntime;

namespace SagaChallenge3.ResilientFunctionApproach.UnitTests;

public class NoOpTimeoutProvider : ITimeoutProvider
{
    public static NoOpTimeoutProvider Instance { get; } = new();
    public Task RegisterTimeout(string timeoutId, DateTime expiresIn)
        => Task.CompletedTask;

    public Task RegisterTimeout(string timeoutId, TimeSpan expiresIn)
        => Task.CompletedTask;

    public Task CancelTimeout(string timeoutId)
        => Task.CompletedTask;
}