using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LanBackup.WebApp.Controllers
{
  public class PaginatedList<T, U, M>
  {
    [JsonProperty(PropertyName = "pi")]
    public int PageIndex { get; private set; }
    [JsonProperty(PropertyName = "tp")]
    public int TotalPages { get; private set; }
    [JsonProperty(PropertyName = "recs")]
    public List<M> Records { get; private set; } = new List<M>();

    public PaginatedList(List<M> items, int count, int pageIndex, int pageSize)
    {
      PageIndex = pageIndex;
      TotalPages = (int)Math.Ceiling(count / (double)pageSize);
      Records.AddRange(items);
    }

    [JsonProperty(PropertyName = "hp")]
    public bool HasPreviousPage
    {
      get
      {
        return (PageIndex > 1);
      }
    }

    [JsonProperty(PropertyName = "hn")]
    public bool HasNextPage
    {
      get
      {
        return (PageIndex < TotalPages);
      }
    }

    public static async Task<PaginatedList<T, U, M>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize, Func<T, U> order, IMapper mapper)
    {
      var count = await source.CountAsync();
      if (order != null)
      {
        var items = source.OrderByDescending(order).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToAsyncEnumerable();
        var mappedItems = mapper.Map<List<T>, List<M>>(await items.ToList());
        return new PaginatedList<T, U, M>(mappedItems, count, pageIndex, pageSize);
      }
      else
      {
        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        var mappedItems = mapper.Map<List<T>, List<M>>(items);
        return new PaginatedList<T, U, M>(mappedItems, count, pageIndex, pageSize);
      }
    }
  }
}
