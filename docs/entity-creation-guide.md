# Entity Oluşturma Rehberi

## 📋 Giriş

Bu rehber, .NET 8 Clean Architecture projesinde yeni bir entity oluşturmak için gereken adımları gösterir. En basit haliyle **Product** entity'si örneği üzerinden açıklanmıştır.

## 🏗️ Mimari Yapı

Entity oluşturmak için 4 katmanda da dosya oluşturmanız gerekir:
1. **Domain** - Entity sınıfı
2. **Application** - CQRS Commands/Queries ve DTO'lar
3. **Persistence** - Repository ve DbContext
4. **WebAPI** - Controller

---

## 📁 Adım 1: Domain Katmanı

### **1.1 Entity Sınıfı Oluştur**
**Dosya:** `src/backend/Domain/Entities/Product.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Product : BaseEntity<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
```

### **1.2 Operation Claims Oluştur (Opsiyonel)**
**Dosya:** `src/backend/Domain/Constants/ProductsOperationClaims.cs`

```csharp
namespace Domain.Constants;

public static class ProductsOperationClaims
{
    public const string Admin = "Products.Admin";
    public const string Create = "Products.Create";
    public const string Read = "Products.Read";
    public const string Update = "Products.Update";
    public const string Delete = "Products.Delete";
}
```

---

## 📁 Adım 2: Application Katmanı

### **2.1 DTO'lar Oluştur**
**Dosya:** `src/backend/Features/Products/Commands/Create/CreateProductCommand.cs`

```csharp
using Application.Features.Products.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Application.Pipelines.Authorization;

namespace Application.Features.Products.Commands.Create;

public class CreateProductCommand : IRequest<CreatedProductResponse>, ISecuredRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }

    public string[] Roles => [ProductsOperationClaims.Create];
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreatedProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<CreatedProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        Product product = _mapper.Map<Product>(request);
        product.CreatedDate = DateTime.UtcNow;
        
        await _productRepository.AddAsync(product);
        
        CreatedProductResponse response = _mapper.Map<CreatedProductResponse>(product);
        return response;
    }
}
```

### **2.2 Response DTO'ları Oluştur**
**Dosya:** `src/backend/Features/Products/Commands/Create/CreatedProductResponse.cs`

```csharp
namespace Application.Features.Products.Commands.Create;

public class CreatedProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
```

### **2.3 List Query Oluştur**
**Dosya:** `src/backend/Features/Products/Queries/GetList/GetListProductQuery.cs`

```csharp
using Application.Features.Products.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;
using NArchitecture.Core.Application.Pipelines.Authorization;

namespace Application.Features.Products.Queries.GetList;

public class GetListProductQuery : IRequest<GetListResponse<GetListProductListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; }

    public string[] Roles => [ProductsOperationClaims.Read];

    public GetListProductQuery()
    {
        PageRequest = new PageRequest { PageIndex = 0, PageSize = 10 };
    }

    public GetListProductQuery(PageRequest pageRequest)
    {
        PageRequest = pageRequest;
    }
}

public class GetListProductQueryHandler : IRequestHandler<GetListProductQuery, GetListResponse<GetListProductListItemDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetListProductQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<GetListResponse<GetListProductListItemDto>> Handle(
        GetListProductQuery request,
        CancellationToken cancellationToken
    )
    {
        IPaginate<Product> products = await _productRepository.GetListAsync(
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        GetListResponse<GetListProductListItemDto> response = _mapper.Map<GetListResponse<GetListProductListItemDto>>(products);
        return response;
    }
}
```

### **2.4 List Item DTO Oluştur**
**Dosya:** `src/backend/Features/Products/Queries/GetList/GetListProductListItemDto.cs`

```csharp
namespace Application.Features.Products.Queries.GetList;

public class GetListProductListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
}
```

### **2.5 Mapping Profile Oluştur**
**Dosya:** `src/backend/Features/Products/Profiles/MappingProfiles.cs`

```csharp
using Application.Features.Products.Commands.Create;
using Application.Features.Products.Queries.GetList;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Products.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<CreateProductCommand, Product>().ReverseMap();
        CreateMap<Product, CreatedProductResponse>().ReverseMap();
        CreateMap<Product, GetListProductListItemDto>().ReverseMap();
    }
}
```

---

## 📁 Adım 3: Persistence Katmanı

### **3.1 Repository Interface Oluştur**
**Dosya:** `src/backend/Application/Services/Repositories/IProductRepository.cs`

```csharp
using Application.Services.Repositories;
using Domain.Entities;

namespace Application.Services.Repositories;

public interface IProductRepository : IAsyncRepository<Product>
{
    // Özel metodlar buraya eklenebilir
}
```

### **3.2 Repository Implementation Oluştur**
**Dosya:** `src/backend/Persistence/Repositories/ProductRepository.cs`

```csharp
using Application.Services.Repositories;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class ProductRepository : EfRepositoryBase<Product, Guid>, IProductRepository
{
    public ProductRepository(BaseDbContext context) : base(context)
    {
    }
}
```

### **3.3 DbContext'e Entity Ekle**
**Dosya:** `src/backend/Persistence/Contexts/BaseDbContext.cs`

```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Contexts;

public class BaseDbContext : DbContext
{
    // Mevcut entity'ler...
    public DbSet<Product> Products { get; set; }

    public BaseDbContext(DbContextOptions<BaseDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Product entity'si için konfigürasyon
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Stock).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
    }
}
```

---

## 📁 Adım 4: WebAPI Katmanı

### **4.1 Controller Oluştur**
**Dosya:** `src/backend/WebAPI/Controllers/ProductsController.cs`

```csharp
using Application.Features.Products.Commands.Create;
using Application.Features.Products.Queries.GetList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Products.Create")]
    public async Task<IActionResult> Add([FromBody] CreateProductCommand createProductCommand)
    {
        CreatedProductResponse response = await Mediator.Send(createProductCommand);
        return Created(uri: "", response);
    }

    [HttpGet]
    [Authorize(Roles = "Products.Read")]
    public async Task<IActionResult> GetList([FromQuery] GetListProductQuery getListProductQuery)
    {
        GetListResponse<GetListProductListItemDto> response = await Mediator.Send(getListProductQuery);
        return Ok(response);
    }
}
```

---

## 📁 Adım 5: Dependency Injection

### **5.1 Service Registration**
**Dosya:** `src/backend/Application/ApplicationServiceRegistration.cs` (mevcut dosyaya ekle)

```csharp
// Mevcut servislerin sonuna ekle
services.AddScoped<IProductRepository, ProductRepository>();
```

---

## 🚀 Kullanım

### **API Endpoint'leri:**

#### **Product Oluştur**
```http
POST /api/products
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json

{
  "name": "Laptop",
  "description": "Gaming laptop",
  "price": 1500.00,
  "stock": 50,
  "isActive": true
}
```

#### **Product Listesi**
```http
GET /api/products?pageIndex=0&pageSize=10
Authorization: Bearer <JWT_TOKEN>
```

---

## 📋 Kontrol Listesi

### **✅ Domain Katmanı:**
- [ ] Entity sınıfı (`Product.cs`)
- [ ] Operation claims (opsiyonel)

### **✅ Application Katmanı:**
- [ ] Command/Query sınıfları
- [ ] Response DTO'ları
- [ ] Mapping profile
- [ ] Repository interface

### **✅ Persistence Katmanı:**
- [ ] Repository implementation
- [ ] DbContext konfigürasyonu

### **✅ WebAPI Katmanı:**
- [ ] Controller
- [ ] Dependency injection

### **✅ Test:**
- [ ] API endpoint'leri test et
- [ ] Swagger'da kontrol et

---

## 🎯 İpuçları

### **📝 Adlandırma Standartları:**
- Entity: `Product`
- Command: `CreateProductCommand`
- Query: `GetListProductQuery`
- Response: `CreatedProductResponse`
- Repository: `IProductRepository`, `ProductRepository`

### **🔗 İlişkiler:**
```csharp
public class Product : BaseEntity<Guid>
{
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } // Navigation property
}
```

### **⚡ Performans:**
- Repository'ler için async metodlar kullan
- Pagination için `PageRequest` kullan
- Lazy loading yerine eager loading

### **🔒 Güvenlik:**
- Her endpoint için authorization ekle
- Role-based access control kullan
- Input validation ekle

---

## 🚨 Common Issues

### **1. Mapping Hatası:**
```csharp
// Mapping profile'de reverse map unutulmuş
CreateMap<CreateProductCommand, Product>().ReverseMap(); // ReverseMap() önemli
```

### **2. Repository Hatası:**
```csharp
// Repository interface'i unutulmuş
public interface IProductRepository : IAsyncRepository<Product> { }
```

### **3. DbContext Hatası:**
```csharp
// DbSet eklenmemiş
public DbSet<Product> Products { get; set; }
```

---

## 🎉 Sonuç

Bu adımları takip ederek projenize yeni entity'ler ekleyebilirsiniz. **Product** örneği en basit haliyle tüm katmanları kapsamaktadır.

**Önemli:** Her entity için aynı yapıyı tekrarlayın ve naming conventions'a sadık kalın.
