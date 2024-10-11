// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;

static class StreamUsage
{
    public static readonly Guid Live = new("E30E6525-2202-4502-8101-5A75EE15F04B");

    public static readonly Guid Archiving = new("BAC8D8BF-EE2A-41ac-B62C-3C2DD00020A5");

    public static readonly Guid Export = new("677AC8DF-09C1-4cde-94FD-AD7146533802");

    public static readonly Guid HighRes = new("CF7BFE49-9DF9-450b-AC83-12025F70B0D5");

    public static readonly Guid LowRes = new("3403C910-32B3-4f9a-B553-C7DC689A78BF");

    public static readonly Guid Remote = new("98BF3D2A-259C-4902-9F8C-5C15CCC57252");

    public static readonly Guid EdgePlayback = new("8EE4B8E0-8CE2-4552-89D7-60C2B9324C5E");
}