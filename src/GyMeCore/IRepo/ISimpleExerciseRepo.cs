﻿using GymAppCore.Models.Entities;

namespace GymAppCore.IRepo;

public interface ISimpleExerciseRepo
{
    Task<SimpleExercise?> Get(Guid id);
    Task<List<SimpleExercise>> GetAll(Guid exerciseId, int page, int size);
    Task<bool> Create(SimpleExercise exercise);
    Task<bool> Update(SimpleExercise exercise);
    Task<bool> Remove(SimpleExercise exercise);
}