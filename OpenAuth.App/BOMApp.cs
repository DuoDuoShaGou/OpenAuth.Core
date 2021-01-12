using OpenAuth.App.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace OpenAuth.App
{
    public class BOMApp : ERPBaseApp<TF_BOM>
    {
        
        public BOMApp(IERPUnitWork unitWork, IERPRepository<TF_BOM> repository,
             IAuth auth) : base(unitWork, repository, auth)
        {
        }

        //public TableData Load()
        //{
        //    var loginUser = _auth.GetCurrentUser();


        //    IQueryable<TF_BOM> query = UnitWork.Find<TF_BOM>(null);

        //    var userOrgs = from user in query
        //                   join relevance in UnitWork.Find<Relevance>(u => u.Key == "UserOrg")
        //                       on user.Id equals relevance.FirstId into temp
        //                   from r in temp.DefaultIfEmpty()
        //                   join org in UnitWork.Find<Org>(null)
        //                       on r.SecondId equals org.Id into orgtmp
        //                   from o in orgtmp.DefaultIfEmpty()
        //                   select new
        //                   {
        //                       user.Account,
        //                       user.Name,
        //                       user.Id,
        //                       user.Sex,
        //                       user.Status,
        //                       user.BizCode,
        //                       user.CreateId,
        //                       user.CreateTime,
        //                       user.TypeId,
        //                       user.TypeName,
        //                       r.Key,
        //                       r.SecondId,
        //                       OrgId = o.Id,
        //                       OrgName = o.Name
        //                   };

        //    //如果请求的orgId不为空
        //    if (!string.IsNullOrEmpty(request.orgId))
        //    {
        //        var org = loginUser.Orgs.SingleOrDefault(u => u.Id == request.orgId);
        //        var cascadeId = org.CascadeId;

        //        var orgIds = loginUser.Orgs.Where(u => u.CascadeId.Contains(cascadeId)).Select(u => u.Id).ToArray();

        //        //只获取机构里面的用户
        //        userOrgs = userOrgs.Where(u => u.Key == Define.USERORG && orgIds.Contains(u.OrgId));
        //    }
        //    else  //todo:如果请求的orgId为空，即为跟节点，这时可以额外获取到机构已经被删除的用户，从而进行机构分配。可以根据自己需求进行调整
        //    {
        //        var orgIds = loginUser.Orgs.Select(u => u.Id).ToArray();

        //        //获取用户可以访问的机构的用户和没有任何机构关联的用户（机构被删除后，没有删除这里面的关联关系）
        //        userOrgs = userOrgs.Where(u => (u.Key == Define.USERORG && orgIds.Contains(u.OrgId)) || (u.OrgId == null));
        //    }



        //    var userViews = userOrgs.ToList().GroupBy(b => b.Account).Select(u => new UserView
        //    {
        //        Id = u.First().Id,
        //        Account = u.Key,
        //        Name = u.First().Name,
        //        Sex = u.First().Sex,
        //        Status = u.First().Status,
        //        CreateTime = u.First().CreateTime,
        //        CreateUser = u.First().CreateId,
        //        OrganizationIds = string.Join(",", u.Select(x => x.OrgId))
        //        ,
        //        Organizations = string.Join(",", u.Select(x => x.OrgName))

        //    });

        //    return new TableData
        //    {
        //        count = userViews.Count(),
        //        data = userViews.OrderBy(u => u.Name)
        //            .Skip((request.page - 1) * request.limit)
        //            .Take(request.limit),
        //    };
        //}

        
        public List<BOMView> ConvertToSelectModel(DataTable dt)
        {
            // 定义集合    
            List<BOMView> ts = new List<BOMView>();

            if (dt == null)
            {
                return ts;
            }

            // 获得此模型的类型   
            Type type = typeof(BOMView);
            string tempName = "";

            foreach (DataRow dr in dt.Rows)
            {
                BOMView t = new BOMView();
                // 获得此模型的公共属性      
                PropertyInfo[] propertys = t.GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;  // 检查DataTable是否包含此列    

                    if (dt.Columns.Contains(tempName))
                    {
                        // 判断此属性是否有Setter      
                        if (!pi.CanWrite) continue;

                        object value = dr[tempName];
                        if (value != DBNull.Value)
                            pi.SetValue(t, value.ToString(), null);
                    }
                }
                ts.Add(t);
            }
            return ts;
        }


    }
}
