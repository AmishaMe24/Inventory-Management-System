using AutoMapper;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Mapping
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			// Product mappings
			CreateMap<CreateUpdateProductDto, Product>();
			CreateMap<Product, ProductDto>();

			// Order mappings
			CreateMap<CreateOrderDto, Order>();
            CreateMap<UpdateOrderDto, Order>();
            CreateMap<Order, OrderDto>();
			CreateMap<OrderItemDto, OrderItem>();
			CreateMap<OrderItem, OrderItemDto>();
		}
	}
}