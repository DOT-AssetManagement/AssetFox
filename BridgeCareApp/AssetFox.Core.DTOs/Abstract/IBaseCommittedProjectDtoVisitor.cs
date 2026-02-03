using System;
using System.Collections.Generic;
using System.Text;

namespace AssetFox.Core.DTOs.Abstract
{
    public interface IBaseCommittedProjectDtoVisitor<THelper, TOutput>
    {
        TOutput Visit(SectionCommittedProjectDTO dto, THelper helper);
    }
}
