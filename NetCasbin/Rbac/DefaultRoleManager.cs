﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCasbin.Rbac
{
    public class DefaultRoleManager : IRoleManager
    {
        private const string _domainError = "error: domain should be 1 parameter";
        private readonly Dictionary<string, Role> _allRoles;
        private readonly int _maxHierarchyLevel;

        public DefaultRoleManager(int maxHierarchyLevel)
        {
            _allRoles = new Dictionary<string, Role>();
            _maxHierarchyLevel = maxHierarchyLevel;
        }

        private bool HasRole(string name)
        {
            return _allRoles.ContainsKey(name);
        }


        public void AddLink(string name1, string name2, params string[] domain)
        {
            if (domain.Length == 1)
            {
                name1 = $"{domain[0]}::{name1}";
                name2 = $"{domain[0]}::{name2}";
            }
            else if (domain.Length > 1)
            {
                throw new ArgumentException(_domainError);
            }

            var role1 = CreateRole(name1);
            var role2 = CreateRole(name2);
            role1.AddRole(role2);
        }

        public virtual void Clear()
        {
            _allRoles.Clear();
        }

        public virtual void DeleteLink(string name1, string name2, params string[] domain)
        {
            if (domain.Length == 1)
            {
                name1 = PrefixDomain(domain[0], name1);
                name2 = PrefixDomain(domain[0], name2);
            }
            else if (domain.Length > 1)
            {
                throw new ArgumentException(_domainError);
            }

            if (!HasRole(name1) || !HasRole(name2))
            {
                return;
            }

            var role1 = CreateRole(name1);
            var role2 = CreateRole(name2);
            role1.DeleteRole(role2);
        }

        public virtual List<string> GetRoles(string name, params string[] domain)
        {
            if (domain.Length == 1)
            {
                name = domain[0] + "::" + name;
            }
            else if (domain.Length > 1)
            {
                throw new Exception("error: domain should be 1 parameter");
            }

            if (!HasRole(name))
            {
                return new List<string>();
            }

            var roles = CreateRole(name).GetRoles().ToList();
            if (domain.Length == 1)
            {
                roles = roles.Select(x => x.Substring(domain[0].Length + 2)).ToList();
            }
            return roles;
        }


        public virtual bool HasLink(string name1, string name2, params string[] domain)
        {
            if (domain.Length == 1)
            {
                name1 = PrefixDomain(domain[0], name1);
                name2 = PrefixDomain(domain[0], name2);
            }
            else if (domain.Length > 1)
            {
                throw new ArgumentException(_domainError);
            }

            if (name1.Equals(name2))
            {
                return true;
            }

            if (!HasRole(name1) || !HasRole(name2))
            {
                return false;
            }

            var role1 = CreateRole(name1);
            return role1.HasRole(name2, _maxHierarchyLevel);
        }

        public virtual List<string> GetUsers(string name, params string[] domain)
        {
            if (domain.Length == 1)
            {
                name = domain[0] + "::" + name;
            }
            else if (domain.Length > 1)
            {
                throw new Exception("error: domain should be 1 parameter");
            }
            if (!HasRole(name))
            {
                //return [];
            }

            var users = _allRoles.Values.Where(x => x.HasDirectRole(name)).Select(x => x.Name);
            if (domain.Length == 1)
            {
                users = users.Select(n => n.Substring(domain[0].Length + 2, n.Length));
            }
            return users.ToList();
        }


        private Role CreateRole(string name)
        {
            if (HasRole(name))
            {
                return _allRoles[name];
            }
            else
            {
                var role = new Role(name);
                _allRoles[name] = role;
                return role;
            }
        }

        protected string PrefixDomain(string domain, string name)
        {
            return $"{domain}::{name}";
        }

    }
}
