using System;
using System.Collections.Generic;
using System.Linq;

namespace Research.Core.Domain.Blogs
{
    // Có thể đưa cài đặt vào lớp BlogPost cũng được
    public static class BlogExtensions
    {
        /// <summary>
        /// Chuyển chuỗi Tags của blogPost thành 1 mảng các tag riêng lẻ
        /// </summary>
        public static string[] ParseTags(this BlogPost blogPost)
        {
            if(!string.IsNullOrWhiteSpace(blogPost.Tags))
            {
                var parsedTags = blogPost.Tags
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToArray();
                return parsedTags;
            }
            return new string[0];
        }
    }
}
