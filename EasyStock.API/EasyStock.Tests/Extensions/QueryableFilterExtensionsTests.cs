using System;
using System.Collections.Generic;
using System.Linq;
using EasyStock.API.Common;
using EasyStock.API.Extensions;
using Xunit;

namespace EasyStock.Tests.Extensions
{
    public class FakeModel
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
    }

    public class QueryableFilterExtensionsTests
    {
        private readonly IQueryable<FakeModel> _data;
        public QueryableFilterExtensionsTests()
        {
            _data = new List<FakeModel>
            {
                new FakeModel { Name = "Apple", Quantity = 10, CreatedOn = new DateTime(2024, 01, 01), IsActive = true },
                new FakeModel { Name = "Banana", Quantity = 5, CreatedOn = new DateTime(2024, 02, 01), IsActive = false },
                new FakeModel { Name = "Cherry", Quantity = 15, CreatedOn = new DateTime(2024, 03, 01), IsActive = true },
                new FakeModel { Name = "apple pie", Quantity = 20, CreatedOn = new DateTime(2024, 01, 15), IsActive = true },
            }.AsQueryable();
        }

        [Fact]
        public void ApplyFilters_StringContains_Works()
        {
            // Arrange
            var filters = new List<FilterCondition>
            {
                new FilterCondition
                {
                    Field = nameof(FakeModel.Name),
                    Operator = "contains",
                    Value = "apple"
                }
            };

            // Act
            var result = _data.ApplyFilters(filters).ToList();

            // Assert
            Assert.Equal(2, result.Count); // Apple and apple pie
        }

        [Fact]
        public void ApplyFilters_NumberGreaterThan_Works()
        {
            var filters = new List<FilterCondition>
            {
                new FilterCondition
                {
                    Field = nameof(FakeModel.Quantity),
                    Operator = ">",
                    Value = "10"
                }
            };

            var result = _data.ApplyFilters(filters).ToList();

            Assert.Equal(2, result.Count); // Quantity > 10 -> Cherry(15) and apple pie(20)
        }

        [Fact]
        public void ApplyFilters_DateEquals_Works()
        {
            var filters = new List<FilterCondition>
            {
                new FilterCondition
                {
                    Field = nameof(FakeModel.CreatedOn),
                    Operator = "equals",
                    Value = "2024-01-01"
                }
            };

            var result = _data.ApplyFilters(filters).ToList();

            Assert.Single(result);
            Assert.Equal("Apple", result[0].Name);
        }

        [Fact]
        public void ApplyFilters_BoolEquals_Works()
        {
            var filters = new List<FilterCondition>
            {
                new FilterCondition
                {
                    Field = nameof(FakeModel.IsActive),
                    Operator = "true",
                    Value = "true"
                }
            };

            var result = _data.ApplyFilters(filters).ToList();

            Assert.Equal(3, result.Count); // Only one entry is inactive
        }

        [Fact]
        public void ApplyFilters_EmptyFilters_ReturnsAll()
        {
            var result = _data.ApplyFilters(new List<FilterCondition>()).ToList();
            Assert.Equal(4, result.Count);
        }

        [Fact]
        public void ApplyFilters_StringStartsWith_Works()
        {
            var filters = new List<FilterCondition>
            {
                new FilterCondition { Field = nameof(FakeModel.Name), Operator = "startswith", Value = "Ban" }
            };

            var result = _data.ApplyFilters(filters).ToList();

            Assert.Single(result);
            Assert.Equal("Banana", result[0].Name);
        }

        [Fact]
        public void ApplyFilters_StringEndsWith_Works()
        {
            var filters = new List<FilterCondition>
            {
                new FilterCondition { Field = nameof(FakeModel.Name), Operator = "endswith", Value = "rry" }
            };

            var result = _data.ApplyFilters(filters).ToList();

            Assert.Single(result);
            Assert.Equal("Cherry", result[0].Name);
        }

        [Fact]
        public void ApplyFilters_NumberLessThanOrEqual_Works()
        {
            var filters = new List<FilterCondition>
            {
                new FilterCondition { Field = nameof(FakeModel.Quantity), Operator = "<=", Value = "10" }
            };

            var result = _data.ApplyFilters(filters).ToList();

            Assert.Equal(2, result.Count); // Apple (10), Banana (5)
        }

        [Fact]
        public void ApplyFilters_DateGreaterThan_Works()
        {
            var filters = new List<FilterCondition>
            {
                new FilterCondition { Field = nameof(FakeModel.CreatedOn), Operator = ">", Value = "2024-01-31" }
            };

            var result = _data.ApplyFilters(filters).ToList();

            Assert.Equal(2, result.Count); // Banana, Cherry
        }

        [Fact]
        public void ApplyFilters_DateNotEquals_Works()
        {
            var filters = new List<FilterCondition>
            {
                new FilterCondition { Field = nameof(FakeModel.CreatedOn), Operator = "!=", Value = "2024-01-01" }
            };

            var result = _data.ApplyFilters(filters).ToList();

            Assert.Equal(3, result.Count); // everything except Apple
        }

        [Fact]
        public void ApplyFilters_BoolFalse_Works()
        {
            var filters = new List<FilterCondition>
            {
                new FilterCondition { Field = nameof(FakeModel.IsActive), Operator = "false", Value = "false" }
            };

            var result = _data.ApplyFilters(filters).ToList();

            Assert.Single(result);
            Assert.False(result[0].IsActive);
        }

        [Fact]
        public void ApplyFilters_CombinesMultipleFilters_WithAnd()
        {
            var filters = new List<FilterCondition>
            {
                new FilterCondition { Field = nameof(FakeModel.IsActive), Operator = "true", Value = "true" },
                new FilterCondition { Field = nameof(FakeModel.Quantity), Operator = ">", Value = "15" }
            };

            var result = _data.ApplyFilters(filters).ToList();

            Assert.Single(result);
            Assert.Equal("apple pie", result[0].Name);
        }

        [Fact]
        public void ApplyFilters_NullFilters_ReturnsAll()
        {
            var result = _data.ApplyFilters<FakeModel>(null!).ToList();
            Assert.Equal(4, result.Count);
        }

        [Fact]
        public void ApplyFilters_InvalidField_Throws()
        {
            var filters = new List<FilterCondition>
            {
                new FilterCondition { Field = "DoesNotExist", Operator = "equals", Value = "test" }
            };

            Assert.Throws<ArgumentException>(() =>
            {
                _data.ApplyFilters(filters).ToList();
            });
        }

        [Fact]
        public void ApplyFilters_InvalidOperator_Throws()
        {
            var filters = new List<FilterCondition>
            {
                new FilterCondition { Field = nameof(FakeModel.Name), Operator = "invalidOp", Value = "test" }
            };

            Assert.Throws<NotSupportedException>(() =>
            {
                _data.ApplyFilters(filters).ToList();
            });
        }

        [Fact]
        public void ApplyFilters_InvalidDate_Throws()
        {
            var filters = new List<FilterCondition>
            {
                new FilterCondition { Field = nameof(FakeModel.CreatedOn), Operator = "=", Value = "not-a-date" }
            };

            Assert.Throws<ArgumentException>(() =>
            {
                _data.ApplyFilters(filters).ToList();
            });
        }

    }
}
