// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IHaveId
    {
        int Id { get; set; }
    }

    public interface IHaveLongId
    {
        long Id { get; set; }
    }
}
