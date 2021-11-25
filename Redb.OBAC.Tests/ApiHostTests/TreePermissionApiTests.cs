using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Redb.OBAC.Tests.Utils;

namespace Redb.OBAC.Tests.ApiHostTests
{
    /// <summary>
    /// разнообразные перемещения объектов и изменения их свойств, сравнение с эталоном
    /// для каждого теста
    /// - формируем базовое состояние дерева и базовый снапшот атомарных полномочий
    /// - делаем инкрементальное изменение, получаем команду на модификацию базовых полномочий
    /// - применяем команду к базовым полномочиям
    /// - получаем снапшот атомарных полномочий через Repair нового состояния
    /// - сравниваем - repair-версия полномочий и инкрементальная версия должны быть эквивалентны
    /// </summary>
    [TestFixture]
    public class TreePermissionApiTests: TestBase
    {
        [Test]
        public async Task SimpleOps()
        {
            throw new NotImplementedException();
            
            // делаем это через GRPC API!
            
            // перенос узла между наследуемыми ветками
            // перенос между наследуемой и ненаследуемой
            // CRUD на полномочия
            // изменение статуса наследования у промежуточного узла
            // изменение статуса наследования у терминального узла
            // удаление узлов
            // добавление одного и того же полномочия на разных узлах - через userid
            // добавление одного и того же полномочия на разных узлах - через разные groupid
            
        }
        
        [Test]
        public async Task PermInheritance1()
        {
            // todo
            // check permission inheritance
            // check how inheritance works when a folder goes moved
            // check copy inherited permission
            throw new NotImplementedException();
        }
    }
}