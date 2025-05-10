using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Entities;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Modules
{
    public class CreateModuleCommand : IRequest<ModuleResponseDto>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Route { get; set; }
        public int DisplayOrder { get; set; }
        public Guid? ParentId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateModuleCommandHandler : IRequestHandler<CreateModuleCommand, ModuleResponseDto>
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<CreateModuleCommandHandler> _logger;

        public CreateModuleCommandHandler(
            IModuleRepository moduleRepository,
            IAuditService auditService,
            ILogger<CreateModuleCommandHandler> logger)
        {
            _moduleRepository = moduleRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<ModuleResponseDto> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si ya existe un módulo con el mismo código
                var existingModule = await _moduleRepository.GetByCodeAsync(request.Code);
                if (existingModule != null)
                {
                    throw new InvalidOperationException($"Ya existe un módulo con el código '{request.Code}'");
                }

                // Verificar si existe el módulo padre si se especifica
                if (request.ParentId.HasValue)
                {
                    var parentModule = await _moduleRepository.GetByIdAsync(request.ParentId.Value);
                    if (parentModule == null)
                    {
                        throw new InvalidOperationException($"No se encontró el módulo padre con ID '{request.ParentId}'");
                    }
                }

                // Crear nuevo módulo
                var module = new Module(request.Name, request.Description, request.Icon, request.Route, request.DisplayOrder, request.ParentId);
                module.Update(request.Name, request.Description, request.Icon, request.Route, request.IsActive, request.DisplayOrder, request.ParentId);

                // Guardar en la base de datos
                var createdModule = await _moduleRepository.CreateAsync(module);

                // Registrar auditoría
                await _auditService.LogActionAsync(
                    userId: null, // ID del usuario que realiza la acción (null si es sistema)
                    action: "Create",
                    entityName: "Module",
                    entityId: createdModule.Id.ToString(),
                    oldValues: null,
                    newValues: new { Name = createdModule.Name, Description = createdModule.Description },
                    ipAddress: null,
                    userAgent: null);

                // Mapear a DTO de respuesta
                return new ModuleResponseDto
                {
                    Id = createdModule.Id,
                    Name = createdModule.Name,
                    Code = createdModule.Name, // Usamos Name como Code ya que la entidad Module no tiene la propiedad Code
                    Description = createdModule.Description,
                    Icon = createdModule.Icon,
                    Route = createdModule.Route,
                    DisplayOrder = createdModule.DisplayOrder,
                    ParentId = createdModule.ParentId,
                    IsActive = createdModule.IsActive,
                    CreatedAt = createdModule.CreatedAt,
                    Success = true,
                    Message = $"Módulo '{createdModule.Name}' creado exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el módulo {ModuleName}", request.Name);
                throw;
            }
        }
    }
}
