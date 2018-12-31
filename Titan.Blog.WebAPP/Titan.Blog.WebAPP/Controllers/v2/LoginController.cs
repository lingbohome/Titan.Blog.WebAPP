﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Titan.Blog.AppService.DomainService;
using Titan.Blog.Infrastructure.Data;
using Titan.Blog.Model.DataModel;
using Titan.Blog.WebAPP.Auth.Policys;
using Titan.Blog.WebAPP.Extensions;
using Titan.Blog.WebAPP.Swagger;

namespace Titan.Blog.WebAPP.Controllers.v2
{
    /// <summary>
    /// 版本2的认证模块
    /// </summary>
    [Produces("application/json")]//Swagger可以根据这个来自动选择请求类型
    [CustomRoute(CustomApiVersion.ApiVersions.v2)]
    public class LoginController : ApiControllerBase
    {
        #region 成员、构造函数注入
        private readonly PermissionRequirement _permissionRequirement;
        private readonly AuthorDomainSvc _authorDomainSvc;
        public LoginController(PermissionRequirement permissionRequirement, AuthorDomainSvc authorDomainSvc)
        {
            _permissionRequirement = permissionRequirement;
            _authorDomainSvc = authorDomainSvc;
        }
        #endregion

        #region 授权登录版本2
        /// <summary>
        /// 获取token
        /// </summary>
        /// <param name="userId">账号</param>
        /// <param name="userPassword">密码</param>
        /// <returns></returns>
        [HttpGet("LoginV2", Name = "LoginV2")]
        public OpResult<string> GetJwtToken(string userId, string userPassword)
        {
            SysUser sysUser;
            var op = _authorDomainSvc.VerifyUserInfo(userId, userPassword,out sysUser);
            if (!op.Successed)
            {
                return op;
            }
            var user = op.Message;
            var userName = sysUser.UserId;
            //如果是基于用户的授权策略，这里要添加用户;如果是基于角色的授权策略，这里要添加角色
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Expiration, DateTime.Now.AddSeconds(_permissionRequirement.Expiration.TotalSeconds).ToString()) };
            claims.AddRange(user.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));//数据库中查出来的当前用户的所有角色,号分开，拼接到list里。后面拦截器会根据这个值来筛选他有误权限来访问url。每个接口上有特性标识。

            //用户标识
            //用户标识
            //var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
            //identity.AddClaims(claims);
            //_permissionRequirement这是个配置，启动的时候注入进来的
            //_permissionRequirement.Audience = userName;//这个不能加，加了会报错
            return JwtToken.BuildJwtToken(claims.ToArray(), _permissionRequirement);
        }
        #endregion

        #region 测试
        /// <summary>
        /// 获取人列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("Fuck", Name = "Fuck")]
        public OpResult<List<object>> BlogList()
        {
            var userInfo = UserInfo;
            var data = new List<object>();
            data.Add(new
            {
                BlogName = "1",
                BlogContent = "ioc容器教程"
            });
            data.Add(new
            {
                BlogName = "1",
                BlogContent = "aop"
            });
            data.Add(new
            {
                BlogName = "1",
                BlogContent = "依赖注入"
            });
            return new OpResult<List<object>>(OpResultType.Success, "", data);
        }

        /// <summary>
        /// 根据Id获取人详情
        /// </summary>
        /// <param name="id">博客Id</param>
        /// <returns>博客详情</returns>
        [AllowAnonymous]
        [HttpGet("Fuck/{id}", Name = "FuckById")]
        public OpResult<List<object>> BlogList(Guid id)
        {
            var userInfo = UserInfo;
            var data = new List<object>();
            data.Add(new
            {
                BlogName = "1",
                BlogContent = "ioc容器教程"
            });
            return new OpResult<List<object>>(OpResultType.Success, "", data);
        }
        #endregion
    }
}
