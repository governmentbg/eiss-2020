using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
  /// <summary>
  /// Полугодишен избор на съдии за ВКС VM
  /// </summary>

  public class VksSelectionProtocolVM
  {

    public int Id { get; set; }
    public int Selectionid { get; set; }

 


    public DateTime DateGenerated { get; set; }
    public DateTime? DateSigned { get; set; }
    public string UserGeneratedId { get; set; }
    public string UserGeneratedName{ get; set; }

    public string UserSignedId { get; set; }
    public string UserSignedName { get; set; }
    public bool IsSigned { get; set; }

    public string FileId { get; set; }
    public string FileName { get; set; }

  } 

}
