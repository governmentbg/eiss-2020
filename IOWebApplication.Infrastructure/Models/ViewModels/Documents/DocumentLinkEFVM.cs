// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    /// <summary>
    /// Свързани документ за електронна папк и ход на дело
    /// </summary>
    public class DocumentLinkEFVM
    {
        /// <summary>
        /// Идентификатор на свързан документ
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Идентификатор на документ към който е закачен свързаният документ
        /// </summary>
        public long DocumentId { get; set; }

        /// <summary>
        /// Съд на свързан документ
        /// </summary>
        public string PrevDocumentCourtLabel { get; set; }

        /// <summary>
        /// Направление на свързан документ
        /// </summary>
        public string PrevDocumentDirectionLabel { get; set; }

        /// <summary>
        /// Свързан документ
        /// </summary>
        public string PrevDocumentLabel { get; set; }
    }
}
