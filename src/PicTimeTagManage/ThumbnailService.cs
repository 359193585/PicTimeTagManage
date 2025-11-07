/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  ThumbnailService.cs
 * 命名空间： PicTimeTagManage
 * 描    述:  
 * 
 * 版    本：  V1.0
 * 创 建 者：  liuxin
 * 电子邮件：  359193585@qq.com(leison)
 * 创建时间：  2025/11/6 14:27
 * ======================================
 * 历史更新记录
 * 版本：V          修改时间：         修改人：
 * 修改内容：
 * ======================================
*********************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace PicTimeTagManage
{
    #region 三个核心接口:服务、创建、缓存
    public interface IThumbnailService
    {
        Task<Image> LoadThumbnailAsync(string imagePath, int width, int height, object userState = null);
        Task<Image> UpdateThumbnailAsync(string imagePath, int width, int height, object userState = null);
        // 事件：当缩略图加载完成时触发
        event EventHandler<ThumbnailLoadedEventArgs> ThumbnailLoaded;
        bool TryGetCached(string imagePath, out Image thumbnail);
    }
    public interface IThumbnailGenerator
    {
        Image GenerateThumbnail(string imagePath, int width, int height);
    }
    public interface IThumbnailCache
    {
        bool TryGet(string imagePath, out Image thumbnail);
        void Set(string imagePath, Image thumbnail);
        void Clear();
    }

    #endregion
    public class ThumbnailLoadedEventArgs : EventArgs
    {
        // 自定义事件参数，包含加载结果和上下文信息
        public string ImagePath { get; set; }
        public Image Thumbnail { get; set; }
        public bool IsSuccess { get; set; }
        public object UserState { get; set; } // 用于传递额外的上下文，如rowIndex
    }
    public class ThumbnailService : IThumbnailService
    {
        private readonly IThumbnailGenerator _generator;
        private readonly IThumbnailCache _cache;
        private readonly ConcurrentDictionary<string, Task<Image>> _ongoingGenerationTasks = new ConcurrentDictionary<string, Task<Image>>();
        private SynchronizationContext _syncContext;

        public event EventHandler<ThumbnailLoadedEventArgs> ThumbnailLoaded;

        //private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);  ？
        public ThumbnailService(IThumbnailGenerator generator, IThumbnailCache cache)
        {
            _generator = generator;
            _cache = cache;
        }
        /// <summary>
        /// 必须在 Form 构造函数里调用，传入 UI 线程上下文
        /// </summary>
        public void AttachToCurrentSynchronizationContext(SynchronizationContext context)
        {
            _syncContext = context
                ?? throw new ArgumentNullException(nameof(context));

            //_syncContext = SynchronizationContext.Current ?? throw new InvalidOperationException("必须在 UI 线程调用 AttachToCurrentSynchronizationContext");
        }

        public async Task<Image> UpdateThumbnailAsync(string imagePath, int width, int height, object userState = null)
        {
            var generationTask = _ongoingGenerationTasks.GetOrAdd(imagePath, _ =>
            Task.Run(() => _generator.GenerateThumbnail(imagePath, width, height)));
            try
            {
                var thumbnail = await generationTask;
                _cache.Set(imagePath, thumbnail);
                RaiseThumbnailLoaded(imagePath, thumbnail, true, userState);
                return thumbnail;
            }
            catch (Exception)
            {
                RaiseThumbnailLoaded(imagePath, Properties.Resources.ErrorImage, false, userState);
                throw;
            }
            finally
            {
                _ongoingGenerationTasks.TryRemove(imagePath, out _);
            }
        }
        public async Task<Image> LoadThumbnailAsync(string imagePath, int width, int height, object userState = null)
        {
            if (_cache.TryGet(imagePath, out var cached))
            {
                RaiseThumbnailLoaded(imagePath, cached, true, userState);
                return cached;
            }
            return await UpdateThumbnailAsync(imagePath, width, height, userState);
        }
        public void ClearCache()
        {
            if (_cache is DictionaryThumbnailCache dictCache)
            {
                dictCache.Clear(); 
            }
        }
        public void Dispose()
        {
            foreach (var task in _ongoingGenerationTasks.Values)
            {
                // Task 无法直接取消，如果需要可使用 CancellationToken
            }
            _ongoingGenerationTasks.Clear();
            ClearCache();
        }
        public bool TryGetCached(string imagePath, out Image thumbnail)
        {
            return _cache.TryGet(imagePath, out thumbnail);
        }
        private void RaiseThumbnailLoaded(string path, Image image, bool success, object state)
        {
            var args = new ThumbnailLoadedEventArgs
            {
                ImagePath = path,
                Thumbnail = image,
                IsSuccess = success,
                UserState = state
            };
            // 事件回到 UI 线程
            _syncContext.Post(_ =>
            {
                ThumbnailLoaded?.Invoke(this, args);
            }, null);
        }

    }
    public class DictionaryThumbnailCache : IThumbnailCache
    {
        private readonly Dictionary<string, Image> _cache = new Dictionary<string, Image>();
        private readonly object _lock = new object();

        public bool TryGet(string imagePath, out Image thumbnail)
        {
            lock (_lock)
            {
                return _cache.TryGetValue(imagePath, out thumbnail);
            }
        }
        public void Set(string imagePath, Image thumbnail)
        {
            lock (_lock)
            {
                _cache[imagePath] = thumbnail;
            }
        }
        public void Clear()
        {
            lock (_lock)
            {
                foreach (var img in _cache.Values)
                    img.Dispose();
                _cache.Clear();
            }
        }
    }
    public class DefaultThumbnailGenerator : IThumbnailGenerator
    {
        public Image GenerateThumbnail(string imagePath, int width, int height)
        {
            var generator = new GenerateThumbnail(false); // 真正的创建略缩图的类
            return generator.GetThumbnailImg(imagePath, width, height);
        }
    }
}

   

