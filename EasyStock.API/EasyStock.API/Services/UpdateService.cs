using AutoMapper;
using DocumentFormat.OpenXml.Vml.Office;
using EasyStock.API.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyStock.API.Services
{
    public class UpdateService<T>: IUpdateService<T> where T: ModelBase, IEntity
    {
        private readonly IMapper _mapper;

        public UpdateService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public T MapAndUpdateAuditFields(T oldEntity, T newEntity, string userName)
        {
            var crDate = oldEntity.CrDate;
            var crUserId = oldEntity.CrUserId;
            var blDate = oldEntity.BlDate;
            var blUserId = oldEntity.BlUserId;

            _mapper.Map(newEntity, oldEntity);

            oldEntity.CrDate = crDate;
            oldEntity.CrUserId = crUserId;
            oldEntity.BlDate = blDate;
            oldEntity.BlUserId = blUserId;
            oldEntity.LcUserId = userName;
            oldEntity.LcDate = DateTime.UtcNow;

            return oldEntity;
        }
    }
}
