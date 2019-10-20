using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace EntityFrameworkCore.DbContextBackedMock.Moq.Tests
{
    [DebuggerDisplay("{nameof(Id)}: {Id}")]
    public class TestEntity3
    {
        public TestEntity3() { }

        public TestEntity3(Guid id)
        {
            Id = id;
        }

        [Key] public Guid Id { get; set; }
    }
}