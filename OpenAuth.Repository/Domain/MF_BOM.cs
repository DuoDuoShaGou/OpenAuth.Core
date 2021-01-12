using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository.Domain
{
   public class MF_BOM
    {
        //BOM_NO	NAME	PRD_NO	PRD_MARK	PF_NO	WH_NO	PRD_KND	UNIT	
        public string BOM_NO { get; set; }
        public string NAME { get; set; }
        public string PRD_NO { get; set; }
        public string PRD_MARK { get; set; }
        public string PF_NO { get; set; }
        public string WH_NO { get; set; }
        public string PRD_KND { get; set; }
        public string UNIT { get; set; }
        //QTY	QTY1	CST_MAKE	CST_PRD	CST_MAN	CST_OUT	USED_TIME	CST	USR_NO	TREE_STRU
        public decimal? QTY { get; set; }
        public decimal? QTY1 { get; set; }
        public decimal? CST_MAN { get; set; }
        public decimal? CST_OUT { get; set; }
        public decimal? USED_TIME { get; set; }
        public decimal? CST { get; set; }
        public string USR_NO { get; set; }
        public string DEP { get; set; }
        public string PHOTO_BOM { get; set; }
        public string EC_NO { get; set; }
        public DateTime? VALID_DD { get; set; }
        public DateTime? END_DD { get; set; }
        //DEP	PHOTO_BOM	EC_NO	VALID_DD	END_DD	REM	USR	CHK_MAN	PRT_SW	CPY_SW	CLS_DATE	
        public string REM { get; set; }
        public string USR { get; set; }
        public string CHK_MAN { get; set; }
        public string PRT_SW { get; set; }
        public DateTime? CLS_DATE { get; set; }

        //MOB_ID	LOCK_MAN	SEB_NO	MOD_NO	TIME_CNT	PRT_USR	DEP_INC	SPC	SYS_DATE	
        public string MOB_ID { get; set; }
        public string LOCK_MAN { get; set; }
        public string SEB_NO { get; set; }
        public string MOD_NO { get; set; }
        public decimal? TIME_CNT { get; set; }
        public string PRT_USR { get; set; }
        public string DEP_INC { get; set; }
        public string SPC { get; set; }
        public DateTime? SYS_DATE { get; set; }
        //CST_MAN_ML	CST_MAK_ML	CST_PRD_ML	CST_OUT_ML	CST_ML	BZ_KND	CST_FCP	CANCEL_ID	
        public decimal? CST_MAN_ML { get; set; }
        public decimal? CST_MAK_ML { get; set; }
        public decimal? CST_PRD_ML { get; set; }
        public decimal? CST_OUT_ML { get; set; }
        public decimal? CST_ML { get; set; }
        public string BZ_KND { get; set; }
        public decimal? CST_FCP { get; set; }
        public string CANCEL_ID { get; set; }
        //DEPRO_NO	CUS_NO	PRT_DATE	QTY_C1	QTY_C2	MODIFY_DD	MODIFY_MAN
        public string DEPRO_NO { get; set; }
        public string CUS_NO { get; set; }
        public DateTime? PRT_DATE { get; set; }
        public decimal? QTY_C1 { get; set; }
        public decimal? QTY_C2 { get; set; }
        public DateTime? MODIFY_DD { get; set; }
        public string MODIFY_MAN { get; set; }
    }
}
